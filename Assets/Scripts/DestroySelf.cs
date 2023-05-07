using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroySelf : MonoBehaviour {
    public float destroyTimer;

    public void Start() {
        //TODO make this work for networkObjects
        Destroy(gameObject, destroyTimer);
    }
}