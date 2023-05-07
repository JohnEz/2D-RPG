using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;

public class LobbyMenu : MonoBehaviour {

    [SerializeField]
    private List<LobbyPlayerCard> playerCards;

    [SerializeField]
    private GameObject startGameObject;

    private bool isReady = false;

    public void Start() {
        LobbyManager.Instance.OnLobbyUpdate.AddListener(HandleLobbyUpdated);

        startGameObject.SetActive(LobbyManager.Instance.IsHost());
        startGameObject.GetComponent<Button>().interactable = false;
    }

    private void HandleLobbyUpdated(Lobby lobby) {
        playerCards.ForEach(card => card.Reset());

        if (lobby == null) {
            return;
        }

        int cardIndex = 0;
        bool isEveryoneReady = true;
        lobby.Players.ForEach(player => {
            playerCards[cardIndex].UpdateValues(player);

            if (player.Data[LobbyManager.KEY_IS_READY].Value != "True") {
                isEveryoneReady = false;
            }

            cardIndex++;
        });

        startGameObject.SetActive(LobbyManager.Instance.IsHost());
        startGameObject.GetComponent<Button>().interactable = isEveryoneReady;
    }

    public void LeaveLobby() {
        LobbyManager.Instance.LeaveLobby();
    }

    public void ToggleReadyStatus() {
        LobbyManager.Instance.UpdateReadyStatus(!isReady);
        isReady = !isReady;
    }

    public void StartGame() {
        LobbyManager.Instance.StartGame();
    }
}