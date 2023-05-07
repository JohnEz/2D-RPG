using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class AbilityIcon : MonoBehaviour {
    [SerializeField] private GameObject fadeOut;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI cooldownText;

    [SerializeField] private float hideDuration = .3f;
    [SerializeField] private float showDuration = .3f;

    private Ability myAbility;

    private bool isOnCooldown = false;

    private void Awake() {
        fadeOut.SetActive(false);
        cooldownText.text = "";
    }

    public void SetAbility(Ability ability) {
        myAbility = ability;

        icon.sprite = ability.Icon;

        isOnCooldown = false;
    }

    private void Update() {
        if (!myAbility) {
            return;
        }

        if (isOnCooldown && !myAbility.IsOnCooldown()) {
            isOnCooldown = false;
            ShowIcon();
        } else if (!isOnCooldown && myAbility.IsOnCooldown()) {
            isOnCooldown = true;
            HideIcon();
        }

        if (isOnCooldown) {
            float remainingCooldown = myAbility.GetRemainingCooldown();
            cooldownText.text = remainingCooldown > 1 ? remainingCooldown.ToString("F0") : remainingCooldown.ToString("F1");
        }
    }

    private void HideIcon() {
        fadeOut.SetActive(true);
        transform.DOLocalMoveY(-50f, hideDuration).SetEase(Ease.OutQuart);
    }

    private void ShowIcon() {
        fadeOut.SetActive(false);
        cooldownText.text = "";
        transform.DOLocalMoveY(0f, showDuration).SetEase(Ease.OutQuart);
    }
}