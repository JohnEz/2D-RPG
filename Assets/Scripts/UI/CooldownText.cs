using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CooldownText : MonoBehaviour {

    [SerializeField]
    private float animationLength = 1f;

    [SerializeField]
    private TextMeshProUGUI textRenderer;

    [SerializeField]
    private Transform parentTransform;

    [SerializeField]
    private Image abilityIcon;

    [SerializeField]
    private AnimationCurve scaleCurve;

    [SerializeField]
    private AnimationCurve alphaCurve;

    public void Awake() {
        parentTransform.localScale = Vector3.zero;

        abilityIcon.color = new Color(255, 255, 255, 0);
    }

    public void Setup(Sprite icon, string text) {
        textRenderer.text = text;
        textRenderer.alpha = 0;

        abilityIcon.sprite = icon;

        PlayAnimation();
    }

    private void PlayAnimation() {
        parentTransform.DOLocalMoveY(-100f, animationLength).SetEase(Ease.InQuart).OnComplete(OnAnimationEnd);
        parentTransform.DOScale(Vector3.one, animationLength).SetEase(scaleCurve);

        textRenderer.DOFade(1, animationLength).SetEase(alphaCurve);
        abilityIcon.DOFade(1, animationLength).SetEase(alphaCurve);
    }

    private void OnAnimationEnd() {
        Destroy(gameObject);
    }
}