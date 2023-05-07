using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class StatusBarController : MonoBehaviour {

    [SerializeField]
    private TextMeshProUGUI statusText;

    [SerializeField]
    private Image statusBar;

    private bool isDisplayed = false;

    private Buff currentStatus;

    private BuffController myBuffs;

    private void Awake() {
        ResetStatus();

        myBuffs = GetComponentInParent<BuffController>();

        myBuffs.OnBuffsChanged.AddListener(HandleBuffsChanged);
    }

    private void HandleBuffsChanged(List<Buff> buffs) {
        if (buffs.Count == 0) {
            ResetStatus();
            return;
        }

        // TODO add a buff priority (eg short stuns should be shown over longer slows)
        Buff buffToDisplay = buffs.OrderByDescending(buff => buff.RemainingTime()).ToList().First();

        SetStatus(buffToDisplay);
    }

    private void SetStatus(Buff newStatus) {
        ResetStatus();
        currentStatus = newStatus;

        statusBar.fillAmount = ((float)currentStatus.Duration - currentStatus.ElapsedTime) / currentStatus.Duration;
        statusText.text = currentStatus.Name;
    }

    private void ResetStatus() {
        statusBar.fillAmount = 0;
        statusText.text = "";
        currentStatus = null;
        isDisplayed = false;
    }

    private void Update() {
        if (!currentStatus) {
            return;
        }

        float fillAmount = ((float)currentStatus.Duration - currentStatus.ElapsedTime) / currentStatus.Duration;
        statusBar.fillAmount = fillAmount;

        if (fillAmount <= 0) {
            ResetStatus();
        }
    }
}