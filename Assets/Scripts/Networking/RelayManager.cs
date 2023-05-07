using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class RelayManager : Singleton<RelayManager> {

    // TODO move this out of relay?
    protected void SetConnectionPayload(string playerId) {
        var payload = JsonUtility.ToJson(new ConnectionPayload() {
            playerId = playerId,
            isDebug = Debug.isDebugBuild
        });

        var payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);

        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
    }

    public async Task<string> CreateRelay(string playerId) {
        SetConnectionPayload(playerId);

        try {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            NetworkManager.Singleton.StartHost();

            return joinCode;
        } catch (RelayServiceException e) {
            print(e);
            return null;
        }
    }

    public async Task<bool> JoinRelay(string joinCode, string playerId) {
        SetConnectionPayload(playerId);

        try {
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.HostConnectionData
            );

            return NetworkManager.Singleton.StartClient();
        } catch (RelayServiceException e) {
            print(e);
            return false;
        }
    }
}