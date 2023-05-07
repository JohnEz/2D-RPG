using UnityEngine;
using System.Collections;
using System;

[Serializable]
[CreateAssetMenu(fileName = "New LeapAbility", menuName = "RPG/Ability/LeapAbility")]
public class LeapAbility : Ability {
    public float MaxDistance = 10f;
    public float LeapDuration;
    public AnimationCurve LeapMoveCurve;
    public AnimationCurve LeapZCurve;
}