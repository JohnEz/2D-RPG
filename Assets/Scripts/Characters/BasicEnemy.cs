using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BasicEnemy : Character {

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();

        SetBaseStats(200, 6f);
    }
}