using UnityEngine;
using System.Collections;
using DG.Tweening;

public class SideBar : MonoBehaviour {

    [SerializeField]
    private bool startShown = false;

    private bool isShown;

    [SerializeField]
    private RectTransform target;

    private float openXPosition;

    private float closedXPosition = 0;

    public void Awake() {
        isShown = startShown;
        openXPosition = target.sizeDelta.x + 2;
        closedXPosition = 0;

        target.anchoredPosition = new Vector2(startShown ? openXPosition : closedXPosition, 0);
        target.gameObject.SetActive(isShown);
    }

    public void ShowSideBar() {
        if (isShown) {
            return;
        }
        target.gameObject.SetActive(true);

        target.DOAnchorPosX(openXPosition, .3f).SetEase(Ease.OutSine);
    }

    public void HideSideBar() {
        if (!isShown) {
            return;
        }

        target.DOAnchorPosX(closedXPosition, .3f).SetEase(Ease.OutSine).OnComplete(HandleSideBarHidden);
    }

    private void HandleSideBarHidden() {
        target.gameObject.SetActive(false);
    }
}