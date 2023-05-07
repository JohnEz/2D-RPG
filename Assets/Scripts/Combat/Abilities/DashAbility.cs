using UnityEngine;
using System.Collections;
using System;

[Serializable]
[CreateAssetMenu(fileName = "New DashAbility", menuName = "RPG/Ability/DashAbility")]
public class DashAbility : Ability {
    public float DashDuration;
    public float DashSpeed;
    public AnimationCurve DashSpeedCurve;

    public GameObject DashHitbox;
}