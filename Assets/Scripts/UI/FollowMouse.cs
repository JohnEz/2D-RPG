using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMouse : MonoBehaviour {

    private void Awake() {
        transform.position = InputHandler.Instance.MouseWorldPosition;
    }

    // Update is called once per frame
    private void Update() {
        transform.position = InputHandler.Instance.MouseWorldPosition;
    }
}