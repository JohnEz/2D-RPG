using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public enum AbilityType {
    MELEE,
    PROJECTILE,
    AOE,
    BUFF,
    HEAL,
    MOVE
}

[Serializable]
[CreateAssetMenu(fileName = "New Ability", menuName = "RPG/Ability/Base")]
public class Ability : ScriptableObject {
    public AbilityType abilityType = AbilityType.MELEE;

    public float castTime;
    public float cooldown;

    [NonSerialized]
    public float timeCast = -Mathf.Infinity;

    [SerializeField]
    public List<TargetGraphic> targetGraphics;

    public delegate void OnHitDelegate(Character hitCharacter, Vector3 hitLocation);

    public OnHitDelegate OnHit;

    public delegate void OnCastCompleteDelegate();

    public OnCastCompleteDelegate OnCastComplete;

    public delegate void OnCompleteDelegate();

    public OnCompleteDelegate OnComplete;

    public AudioClip castSFX;
    public GameObject castVFX;

    public Sprite Icon;

    public float speedWhileCasting = 0.1f;

    [SerializeField]
    public List<GameObject> prefabs;

    public float radius = 1f;

    public bool IsOnCooldown() {
        return timeCast + cooldown >= Time.time;
    }

    public bool CanCast() {
        return !IsOnCooldown();
    }

    public float GetRemainingCooldown() {
        float remainingCooldown = timeCast + cooldown - Time.time;

        return Mathf.Max(remainingCooldown, 0);
    }

    public string GetCooldownAsString() {
        float remainingCooldown = GetRemainingCooldown();
        return remainingCooldown > 1 ? remainingCooldown.ToString("F0") : remainingCooldown.ToString("F1");
    }
}