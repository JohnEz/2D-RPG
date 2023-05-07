using UnityEngine;
using System.Collections;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using System;

[Serializable]
public class ConnectionPayload {
    public string playerId;
    public string playerName;
    public bool isDebug;
}

[Serializable]
public class PlayerSessionData {
    public ulong ClientId { get; set; }
    public string PlayerName { get; set; }
    public int CharacterIndex { get; set; }
    public int PlayerNumber { get; set; }

    public PlayerSessionData(ulong clientId, string name, int characterIndex) {
        ClientId = clientId;
        PlayerName = name;
        CharacterIndex = characterIndex;
        PlayerNumber = -1;
    }
}

public class SessionManager : Singleton<SessionManager> {

    // TODO should i remove this and just pass the data on connection?
    public List<Player> LobbyPlayers { get; set; }

    public Dictionary<string, PlayerSessionData> playerSessions = new Dictionary<string, PlayerSessionData>();
    public Dictionary<ulong, string> clientIdToPlayerId = new Dictionary<ulong, string>();

    private void Start() {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        LobbyManager.Instance.OnLobbyUpdate.AddListener(HandleLobbyUpdate);
    }

    private void OnDestroy() {
        // TODO networkmanager will be destroyed here too so this errors
        //NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        //LobbyManager.Instance.OnLobbyUpdate.RemoveListener(HandleLobbyUpdate);
    }

    private void HandleLobbyUpdate(Lobby updatedLobby) {
        LobbyPlayers = updatedLobby.Players;
    }

    // TODO should this be in a connection manager?
    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) {
        byte[] connectionData = request.Payload;
        ulong clientId = request.ClientNetworkId;

        string payload = System.Text.Encoding.UTF8.GetString(connectionData);
        ConnectionPayload connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);

        SetupPlayerSessionData(clientId, connectionPayload);

        //connection approval
        response.Approved = true;
        //response.CreatePlayerObject = true;
        response.Position = Vector3.zero;
        response.Rotation = Quaternion.identity;
        return;
    }

    // TODO add reconecting
    private void SetupPlayerSessionData(ulong clientId, ConnectionPayload connectionPayload) {
        if (playerSessions.ContainsKey(connectionPayload.playerId)) {
            Debug.LogError("Client with the same player Id connected again");
            return;
        }

        PlayerSessionData newSessionData = CreatePlayerSessionData(clientId, connectionPayload.playerId);

        playerSessions.Add(connectionPayload.playerId, newSessionData);
        clientIdToPlayerId.Add(clientId, connectionPayload.playerId);

        print($"Added player session data for clientId: {newSessionData.ClientId}, playerId: {connectionPayload.playerId}, playerName: {newSessionData.PlayerName}");
    }

    //TODO add player number, calculate max players
    private PlayerSessionData CreatePlayerSessionData(ulong clientId, string playerId) {
        Player lobbyPlayer = LobbyPlayers.Find(player => player.Id.Equals(playerId));

        if (lobbyPlayer == null) {
            Debug.LogError($"Player with id {playerId} was not found in lobby players");
            return null;
        }

        PlayerSessionData newSessionData = new PlayerSessionData(clientId, lobbyPlayer.Data[LobbyManager.KEY_PLAYER_NAME].Value, Int32.Parse(lobbyPlayer.Data[LobbyManager.KEY_PLAYER_CHARACTER].Value));

        return newSessionData;
    }

    public PlayerSessionData GetSessionData(string playerId) {
        if (!playerSessions.ContainsKey(playerId)) {
            print(playerSessions);
            Debug.LogError($"No session data found for playerId {playerId}");
            return null;
        }

        return playerSessions[playerId];
    }

    public PlayerSessionData GetSessionData(ulong clientId) {
        if (!clientIdToPlayerId.ContainsKey(clientId)) {
            print(clientIdToPlayerId);
            Debug.LogError($"No player ID found for clientId {clientId}");
            return null;
        }

        string playerId = clientIdToPlayerId[clientId];
        return GetSessionData(playerId);
    }
}