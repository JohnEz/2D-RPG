using UnityEngine;
using System.Collections;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Threading.Tasks;

[System.Serializable]
public struct SelectableCharacter {

    [SerializeField]
    public string name;

    [SerializeField]
    public Color color;

    [SerializeField]
    public Sprite icon;
}

public class LobbyManager : Singleton<LobbyManager> {
    private const float HEART_BEAT_DELAY = 20f;
    private const float LOBBY_UPDATE_POLL_DELAY = 1.1f;
    private Lobby hostedLobby;
    private Lobby joinedLobby;

    public UnityEvent OnLoggedIn = new UnityEvent();
    public UnityEvent OnLobbyJoined = new UnityEvent();
    public UnityEvent OnLobbyDisconnected = new UnityEvent();
    public UnityEvent<Lobby> OnLobbyUpdate = new UnityEvent<Lobby>();
    public UnityEvent OnGameStarted = new UnityEvent();

    private string playerName;
    private const string DEFAULT_NAME = "Player";

    public const string KEY_PLAYER_NAME = "PlayerName";
    public const string KEY_IS_READY = "IsReady";
    public const string KEY_RELAY_CODE = "RelayCode";
    public const string KEY_PLAYER_CHARACTER = "Character";

    public List<SelectableCharacter> selectableCharacters;

    public static bool IsPlayerMe(Player player) {
        string myId = AuthenticationService.Instance.PlayerId;

        return player.Id.Equals(myId);
    }

    private void Start() {
    }

    private void OnDestroy() {
        if (!this.gameObject.scene.isLoaded) {
            return;
        }

        if (joinedLobby != null) {
            LeaveLobby();
        }

        CancelInvoke("LobbyHeartbeat");
        CancelInvoke("PollForLobbyUpdates");
    }

    public bool IsHost() {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    public async void Authenticate(string givenPlayerName) {
        if (givenPlayerName != "") {
            playerName = givenPlayerName;
        } else {
            int randomId = Random.Range(0, 10000);
            playerName = $"{DEFAULT_NAME}-{randomId}";
        }

        InitializationOptions initializationOptions = new InitializationOptions();
        initializationOptions.SetProfile(playerName);

        await UnityServices.InitializeAsync(initializationOptions);

        AuthenticationService.Instance.SignedIn += () => {
            print($"Signed in {AuthenticationService.Instance.PlayerId}");
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        OnLoggedIn.Invoke();
    }

    private async void LobbyHeartbeat() {
        if (hostedLobby == null) {
            return;
        }

        print("Heartbeat");

        await LobbyService.Instance.SendHeartbeatPingAsync(hostedLobby.Id);
        Invoke("LobbyHeartbeat", HEART_BEAT_DELAY);
    }

    private async void PollForLobbyUpdates() {
        if (joinedLobby == null) {
            return;
        }

        Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
        UpdateLobby(lobby);

        Invoke("PollForLobbyUpdates", LOBBY_UPDATE_POLL_DELAY);

        if (joinedLobby.Data[KEY_RELAY_CODE].Value != "0") {
            if (!IsHost()) {
                await RelayManager.Instance.JoinRelay(joinedLobby.Data[KEY_RELAY_CODE].Value, AuthenticationService.Instance.PlayerId);
            }

            hostedLobby = null;
            joinedLobby = null;
            OnGameStarted.Invoke();
        }
    }

    private void UpdateLobby(Lobby lobby) {
        joinedLobby = lobby;
        OnLobbyUpdate.Invoke(lobby);
    }

    private void SetJoinedLobby(Lobby lobby) {
        UpdateLobby(lobby);

        if (lobby != null) {
            OnLobbyJoined.Invoke();
        } else {
            OnLobbyDisconnected.Invoke();
        }
    }

    public async Task<bool> CreateLobby(string givenLobbyName) {
        string lobbyName = givenLobbyName != "" ? givenLobbyName : $"{playerName}'s Lobby";

        bool createdLobby;

        try {
            int maxPlayers = 4;
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions {
                IsPrivate = false,
                Player = CreatePlayer(),
                Data = new Dictionary<string, DataObject> {
                    { KEY_RELAY_CODE, new DataObject(DataObject.VisibilityOptions.Member, "0") }
                }
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

            hostedLobby = lobby;
            SetJoinedLobby(lobby);

            Invoke("LobbyHeartbeat", HEART_BEAT_DELAY);
            Invoke("PollForLobbyUpdates", LOBBY_UPDATE_POLL_DELAY);

            createdLobby = true;
        } catch (LobbyServiceException e) {
            print(e);
            createdLobby = false;
        }

        return createdLobby;
    }

    public async Task<List<Lobby>> GetLobbies() {
        List<Lobby> activeLobbies = new List<Lobby>();
        try {
            QueryResponse response = await Lobbies.Instance.QueryLobbiesAsync();

            activeLobbies = response.Results;
        } catch (LobbyServiceException e) {
            print(e);
        }

        return activeLobbies;
    }

    public async void JoinLobbyById(string lobbyId) {
        try {
            JoinLobbyByIdOptions joinLobbyByCodeOptions = new JoinLobbyByIdOptions {
                Player = CreatePlayer()
            };
            Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, joinLobbyByCodeOptions);

            SetJoinedLobby(lobby);

            Invoke("PollForLobbyUpdates", LOBBY_UPDATE_POLL_DELAY);
        } catch (LobbyServiceException e) {
            print(e);
        }
    }

    private Player CreatePlayer() {
        return new Player {
            Data = new Dictionary<string, PlayerDataObject> {
                { KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) },
                { KEY_IS_READY, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, false.ToString()) },
                { KEY_PLAYER_CHARACTER, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "0") }
            }
        };
    }

    public async void LeaveLobby() {
        try {
            if (IsHost() && joinedLobby.Players.Count > 1) {
                MigrateLobbyHost();
            }

            // TODO should go in if?
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);

            if (joinedLobby != null) {
                hostedLobby = null;
                SetJoinedLobby(null);
            }
        } catch (LobbyServiceException e) {
            print(e);
        }
    }

    public async void KickPlayer(string playerId) {
        try {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
        } catch (LobbyServiceException e) {
            print(e);
        }
    }

    public async void MigrateLobbyHost() {
        try {
            hostedLobby = await Lobbies.Instance.UpdateLobbyAsync(hostedLobby.Id, new UpdateLobbyOptions {
                HostId = joinedLobby.Players[1].Id
            });

            UpdateLobby(hostedLobby);
        } catch (LobbyServiceException e) {
            print(e);
        }
    }

    public void UpdateReadyStatus(bool isReady) {
        UpdatePlayerOption(KEY_IS_READY, isReady.ToString());
    }

    public void UpdateSelectedCharacter(int newCharacterIndex) {
        UpdatePlayerOption(KEY_PLAYER_CHARACTER, newCharacterIndex.ToString());
    }

    private async void UpdatePlayerOption(string key, string value) {
        UpdatePlayerOptions playerOptionsDelta = new UpdatePlayerOptions {
            Data = new Dictionary<string, PlayerDataObject> {
                { key, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, value) }
            }
        };

        try {
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, playerOptionsDelta);
        } catch (LobbyServiceException e) {
            print(e);
        }
    }

    public async void StartGame() {
        if (!IsHost()) {
            return;
        }

        try {
            string relayCode = await RelayManager.Instance.CreateRelay(AuthenticationService.Instance.PlayerId);

            Lobby updatedLobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions {
                Data = new Dictionary<string, DataObject> {
                    { KEY_RELAY_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                }
            });

            joinedLobby = updatedLobby;
        } catch (LobbyServiceException e) {
            print(e);
        }
    }

    public List<Player> GetPlayersInLobby() {
        return joinedLobby.Players;
    }
}