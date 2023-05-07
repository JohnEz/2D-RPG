using UnityEngine;
using System.Collections;
using System;

public enum TargetGraphicStyle {
    FOLLOW_MOUSE,
    LEAP,
    SELF
}

[Serializable]
public struct TargetGraphic {

    [SerializeField]
    public GameObject prefab;

    [SerializeField]
    public float scale;

    [SerializeField]
    public TargetGraphicStyle myStyle;
}

public class TargetGraphicController : MonoBehaviour {
    private TargetGraphicStyle myStyle;

    public void InitialiseSelfTarget(Transform parent, float scale) {
        myStyle = TargetGraphicStyle.SELF;
        transform.SetParent(parent);
        transform.localPosition = Vector3.zero;
        SetScale(scale);
    }

    public void InitialiseFollowMouseTarget(float scale) {
        myStyle = TargetGraphicStyle.FOLLOW_MOUSE;
        FollowMouse followScript = AddScript(typeof(FollowMouse)) as FollowMouse;
        SetScale(scale);
    }

    public void InitialiseLeapTarget(Transform startTransform, float maxDistance, float scale) {
        myStyle = TargetGraphicStyle.LEAP;
        LeapTarget leapScript = AddScript(typeof(LeapTarget)) as LeapTarget;

        leapScript.StartTransform = startTransform;
        leapScript.MaxDistance = maxDistance;
        SetScale(scale);
    }

    private Component AddScript(System.Type component) {
        return gameObject.AddComponent(component);
    }

    private void SetScale(float scale) {
        transform.localScale = new Vector3(scale, scale, scale);
    }
}