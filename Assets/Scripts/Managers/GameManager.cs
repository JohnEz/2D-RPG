using UnityEngine;
using System.Collections;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[RequireComponent(typeof(NetcodeHooks))]
public class GameManager : Singleton<GameManager> {

    [SerializeField]
    private NetcodeHooks netcodeHooks;

    [SerializeField]
    private List<NetworkObject> playerPrefabs;

    [SerializeField]
    private List<Transform> playerSpawnPoints;

    private bool initialSpawnDone = false;
    private int playerSpawnCount = 0;

    protected void Awake() {
        netcodeHooks.OnNetworkSpawnHook += OnNetworkSpawn;
        netcodeHooks.OnNetworkDespawnHook += OnNetworkDespawn;
    }

    private void OnNetworkSpawn() {
        if (!NetworkManager.Singleton.IsServer) {
            enabled = false;
            return;
        }

        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
        NetworkManager.Singleton.SceneManager.OnSynchronizeComplete += OnSynchronizeComplete;
    }

    private void OnNetworkDespawn() {
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
        NetworkManager.Singleton.SceneManager.OnSynchronizeComplete -= OnSynchronizeComplete;
    }

    private void OnClientDisconnect(ulong clientId) {
    }

    private void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut) {
        if (!initialSpawnDone && loadSceneMode == LoadSceneMode.Single) {
            initialSpawnDone = true;
            foreach (KeyValuePair<ulong, NetworkClient> kvp in NetworkManager.Singleton.ConnectedClients) {
                SpawnPlayer(kvp.Key);
            }
        }
    }

    private void OnSynchronizeComplete(ulong clientId) {
        // TODO check that player hasnt already spawned
        if (initialSpawnDone) {
            SpawnPlayer(clientId);
        }
    }

    private void SpawnPlayer(ulong clientId) {
        // if player already has a spawned object
        //NetworkObject characterPrefab = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);

        PlayerSessionData sessionData = SessionManager.Instance.GetSessionData(clientId);

        NetworkObject newPlayer;

        if (sessionData != null) {
            newPlayer = CreatePlayerFromSessionData(sessionData);
        } else {
            Debug.LogError($"Created a player for clientId {clientId} without session data");
            newPlayer = CreateDefaultPlayer();
        }

        newPlayer.SpawnWithOwnership(clientId, true);
        playerSpawnCount++;
    }

    private NetworkObject CreateDefaultPlayer() {
        NetworkObject newPlayer = Instantiate(playerPrefabs[0]);
        Transform spawnLocation = playerSpawnPoints[playerSpawnCount % playerSpawnPoints.Count];
        newPlayer.transform.position = spawnLocation.position;

        return newPlayer;
    }

    private NetworkObject CreatePlayerFromSessionData(PlayerSessionData sessionData) {
        NetworkObject newPlayer = Instantiate(playerPrefabs[sessionData.CharacterIndex]);
        Transform spawnLocation = playerSpawnPoints[playerSpawnCount % playerSpawnPoints.Count];
        newPlayer.transform.position = spawnLocation.position;

        return newPlayer;
    }
}