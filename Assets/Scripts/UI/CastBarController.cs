using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CastBarController : MonoBehaviour {
    private Image castBar;

    private float castStartTime;
    private float abilityCastTime;
    private bool isCasting = false;

    private void Awake() {
        castBar = GetComponent<Image>();
        castBar.fillAmount = 0;
    }

    private void Update() {
        ProgressSlider();
    }

    public void Initialize(UnitController unitController) {
        unitController.OnCastStart.AddListener(HandleCastStart);
        unitController.OnCastFail.AddListener(HandleCastCancel);
        unitController.OnCastSuccess.AddListener(HandleCastCancel);
    }

    private void ProgressSlider() {
        if (!isCasting) {
            return;
        }

        float timePassed = Time.time - castStartTime;

        float percentComplete = timePassed / abilityCastTime;
        castBar.fillAmount = percentComplete;

        if (percentComplete >= 1) {
            HideBar();
        }
    }

    private void HandleCastStart(Ability ability) {
        isCasting = true;
        castStartTime = Time.time;
        abilityCastTime = ability.castTime;
    }

    private void HandleCastCancel() {
        HideBar();
    }

    private void HideBar() {
        isCasting = false;
        castBar.fillAmount = 0;
    }
}