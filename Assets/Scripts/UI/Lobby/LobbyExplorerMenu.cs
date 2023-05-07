using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;

public class LobbyExplorerMenu : MonoBehaviour {
    private List<LobbyCard> lobbyCards = new List<LobbyCard>();

    [SerializeField]
    private Transform lobbyListTransform;

    [SerializeField]
    private GameObject lobbyCardPrefab;

    public void Start() {
        RefreshLobbyList();
    }

    public async void RefreshLobbyList() {
        List<Lobby> lobbies = await LobbyManager.Instance.GetLobbies();

        lobbyCards.ForEach(card => Destroy(card));
        lobbyCards = new List<LobbyCard>();

        lobbies.ForEach(lobby => {
            GameObject lobbyCardObject = Instantiate(lobbyCardPrefab, lobbyListTransform);
            LobbyCard card = lobbyCardObject.GetComponent<LobbyCard>();

            card.Initialise(lobby.Id, lobby.Name, lobby.Players.Count, lobby.MaxPlayers);
            lobbyCards.Add(card);
        });
    }
}