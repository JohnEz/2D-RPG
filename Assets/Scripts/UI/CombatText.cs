using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class CombatText : MonoBehaviour {
    public float animationLength = 1f;

    public TextMeshProUGUI textRenderer;
    public Transform textTransform;

    public AnimationCurve scaleCurve;
    public AnimationCurve alphaCurve;

    public const float MIN_FONT_SIZE = 0.3f;
    public const float MAX_FONT_SIZE = 0.7f;

    public void Awake() {
        textTransform.localScale = Vector3.zero;
    }

    public void Setup(string text, Color color, float fontSize) {
        textRenderer.text = text;
        textRenderer.color = color;
        textRenderer.alpha = 0;
        textRenderer.fontSize = Mathf.Clamp(fontSize, MIN_FONT_SIZE, MAX_FONT_SIZE);

        PlayAnimation();
    }

    private void PlayAnimation() {
        textTransform.DOLocalMoveY(1.5f, animationLength).SetEase(Ease.InQuart).OnComplete(OnAnimationEnd);
        textTransform.DOScale(Vector3.one, animationLength).SetEase(scaleCurve);

        textRenderer.DOFade(1, animationLength).SetEase(alphaCurve);
    }

    private void OnAnimationEnd() {
        Destroy(gameObject);
    }
}