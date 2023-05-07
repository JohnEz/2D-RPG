using UnityEngine;
using System.Collections;
using TMPro;

public class LobbyCard : MonoBehaviour {

    [SerializeField]
    private TMP_Text lobbyNameText;

    [SerializeField]
    private TMP_Text playerCountText;

    private string lobbyId;

    public void Initialise(string id, string lobbyName, int currentPlayers, int maxPlayers) {
        lobbyId = id;
        lobbyNameText.text = lobbyName;
        playerCountText.text = $"{currentPlayers}/{maxPlayers}";
    }

    public void HandleLobbyClicked() {
        LobbyManager.Instance.JoinLobbyById(lobbyId);
    }
}