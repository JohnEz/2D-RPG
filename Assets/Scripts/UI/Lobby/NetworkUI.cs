using UnityEngine;
using System.Collections;
using TMPro;
using Unity.Netcode;

public class NetworkUI : MonoBehaviour {

    [SerializeField]
    private GameObject loginPanel;

    [SerializeField]
    private GameObject lobbyExplorerPanel;

    [SerializeField]
    private GameObject lobbyPanel;

    private void Start() {
        loginPanel.SetActive(true);
        lobbyExplorerPanel.SetActive(false);
        lobbyPanel.SetActive(false);

        LobbyManager.Instance.OnLoggedIn.AddListener(HandleLoggedIn);
        LobbyManager.Instance.OnLobbyJoined.AddListener(HandleJoinedLobby);
        LobbyManager.Instance.OnLobbyDisconnected.AddListener(HandleDisconnectLobby);

        LobbyManager.Instance.OnGameStarted.AddListener(HandleGameStarted);
    }

    private void OnDestroy() {
        if (LobbyManager.Instance) {
            LobbyManager.Instance.OnLoggedIn.RemoveListener(HandleLoggedIn);
        }
    }

    private void HandleLoggedIn() {
        loginPanel.SetActive(false);
        lobbyExplorerPanel.SetActive(true);
    }

    private void HandleJoinedLobby() {
        lobbyExplorerPanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }

    private void HandleDisconnectLobby() {
        lobbyPanel.SetActive(false);
        lobbyExplorerPanel.SetActive(true);
    }

    private void HandleGameStarted() {
        if (NetworkManager.Singleton.IsHost) {
            NetworkManager.Singleton.SceneManager.LoadScene("GameScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }
}