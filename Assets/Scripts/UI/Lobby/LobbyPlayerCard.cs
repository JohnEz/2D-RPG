using UnityEngine;
using System.Collections;
using Unity.Services.Lobbies.Models;
using TMPro;
using System;
using UnityEngine.UI;

public class LobbyPlayerCard : MonoBehaviour {

    [SerializeField]
    private TMP_Text displayName;

    [SerializeField]
    private GameObject waitingObject;

    [SerializeField]
    private GameObject connectedObject;

    [SerializeField]
    private GameObject readyTickObject;

    [SerializeField]
    private TMP_Text characterName;

    [SerializeField]
    private Image spriteRenderer;

    [SerializeField]
    private GameObject characterSelectButtons;

    private int characterIndex;

    public void Reset() {
        waitingObject.SetActive(true);
        connectedObject.SetActive(false);
        displayName.text = "";
        characterIndex = 0;
        characterSelectButtons.SetActive(false);
    }

    public void UpdateValues(Player player) {
        if (player == null) {
            Reset();
            return;
        }

        characterSelectButtons.SetActive(LobbyManager.IsPlayerMe(player));

        waitingObject.SetActive(false);
        connectedObject.SetActive(true);

        displayName.text = player.Data[LobbyManager.KEY_PLAYER_NAME].Value;

        bool isReady = player.Data[LobbyManager.KEY_IS_READY].Value == "True";
        readyTickObject.SetActive(isReady);

        UpdateSelectedCharacter(Int32.Parse(player.Data[LobbyManager.KEY_PLAYER_CHARACTER].Value));
    }

    private void UpdateSelectedCharacter(int newIndex) {
        characterIndex = newIndex;
        SelectableCharacter selectedCharacter = LobbyManager.Instance.selectableCharacters[characterIndex];

        characterName.text = selectedCharacter.name;
        spriteRenderer.overrideSprite = selectedCharacter.icon;
        spriteRenderer.color = selectedCharacter.color;
    }

    public void NextCharacter() {
        int nextIndex = (characterIndex + 1) % LobbyManager.Instance.selectableCharacters.Count;
        UpdateSelectedCharacter(nextIndex);
        LobbyManager.Instance.UpdateSelectedCharacter(nextIndex);
    }

    public void PreviousCharacter() {
        int nextIndex = ((characterIndex - 1) + LobbyManager.Instance.selectableCharacters.Count) % LobbyManager.Instance.selectableCharacters.Count;
        UpdateSelectedCharacter(nextIndex);
        LobbyManager.Instance.UpdateSelectedCharacter(nextIndex);
    }
}