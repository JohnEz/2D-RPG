using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyPod : MonoBehaviour {
    private List<BaseAI> units;

    private void Awake() {
        units = new List<BaseAI>(GetComponentsInChildren<BaseAI>());
    }

    public void OnAggro(BaseAI caller, Character targetEnemy) {
        units.ForEach(unit => {
            if (caller == unit) {
                return;
            }

            unit.OnAggro(targetEnemy);
        });
    }
}