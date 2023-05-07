using UnityEngine;
using System.Collections;
using TMPro;
using DG.Tweening;

public class LoginMenu : MonoBehaviour {

    [SerializeField]
    private TMP_InputField usernameInput;

    public void Login() {
        LobbyManager.Instance.Authenticate(usernameInput.text);
    }
}