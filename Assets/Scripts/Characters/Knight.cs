using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.VFX;

public class Knight : Character {
    [SerializeField] private Ability longswordObject;
    private Ability longsword;
    [SerializeField] private Ability guardObject;
    private Ability guard;
    [SerializeField] private Ability courageousLeapObject;
    private Ability courageousLeap;
    [SerializeField] private Ability shieldSlamObject;
    private Ability shieldSlam;
    [SerializeField] private Ability tauntObject;
    private Ability taunt;

    [SerializeField] private AudioClip courageousLeapSFX;

    [SerializeField] private AudioClip tauntSFX;

    [SerializeField] private GameObject shieldSlamHitboxPrefab;
    [SerializeField] private AudioClip shieldSlamDashSFX;
    [SerializeField] private AudioClip shieldSlamHitSFX;

    private bool hasGainedShieldSlamShield;
    private bool hasHealedFromLongsword;

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        SetBaseStats(260, 4f);

        longsword = Instantiate(longswordObject);
        guard = Instantiate(guardObject);
        courageousLeap = Instantiate(courageousLeapObject);
        shieldSlam = Instantiate(shieldSlamObject);
        taunt = Instantiate(tauntObject);

        longsword.OnCastComplete = () => LongswordCast(0);
        longsword.OnHit = (Character hitCharacter, Vector3 hitLocation) => OnLongswordHit(longsword, hitCharacter, hitLocation);
        abilities.Add(longsword);

        shieldSlam.OnCastComplete = () => ShieldSlamCast(1);
        shieldSlam.OnHit = (Character hitCharacter, Vector3 hitLocation) => OnShieldSlamHit(longsword, hitCharacter, hitLocation);
        abilities.Add(shieldSlam);

        courageousLeap.OnCastComplete = () => CourageousLeap(2);
        courageousLeap.OnComplete = () => OnCourageousLeapLand(2);
        abilities.Add(courageousLeap);

        guard.OnCastComplete = () => GuardCast(3);
        guard.OnHit = (Character hitCharacter, Vector3 hitLocation) => OnGuardHit(guard, hitCharacter, hitLocation);
        abilities.Add(guard);

        taunt.OnCastComplete = () => TauntCast(4);
        abilities.Add(taunt);
    }

    private void LongswordCast(int abilityIndex) {
        hasHealedFromLongsword = false;

        if (IsOwner) {
            characterActions.CreateSlash(unitController.GetAimDirection(), abilityIndex, 0);
        }
    }

    private void OnLongswordHit(Ability longsword, Character hitCharacter, Vector3 hitLocation) {
        if (!hitCharacter) {
            return;
        }

        int damageMin = 7;
        int damageMax = 11;
        hitCharacter.TakeDamage(Random.Range(damageMin, damageMax + 1));

        if (buffController.HasBuff("Valiant") && !hasHealedFromLongsword) {
            // TODO true healing
            ReceiveHealing(4);
            hasHealedFromLongsword = true;
        }

        // TODO reduce shieldbash cooldown?
    }

    private void ShieldSlamCast(int abilityIndex) {
        DashAbility dashAbility = (DashAbility)abilities[abilityIndex];
        Vector3 dashDirection = unitController.GetAimDirection();

        if (IsOwner) {
            // TODO RPC?
            unitController.StartDashing(dashDirection, dashAbility.DashSpeedCurve, dashAbility.DashDuration, dashAbility.DashSpeed);
        }

        if (NetworkManager.Singleton.IsServer) {
            hasGainedShieldSlamShield = false;
        }

        GameObject shieldSlamVFX = Instantiate(dashAbility.prefabs[0], transform);
        shieldSlamVFX.transform.up = dashDirection;
        Destroy(shieldSlamVFX, dashAbility.DashDuration);

        GameObject hitbox = Instantiate(shieldSlamHitboxPrefab, transform);
        hitbox.transform.up = dashDirection;
        hitbox.GetComponent<Hitbox>().Initialise(faction, dashAbility.OnHit, dashAbility.DashDuration);

        AudioManager.Instance.PlaySound(shieldSlamDashSFX, transform);
    }

    private void OnShieldSlamHit(Ability shieldSlam, Character hitCharacter, Vector3 hitLocation) {
        AudioManager.Instance.PlaySound(shieldSlamHitSFX, hitCharacter.transform);

        if (NetworkManager.Singleton.IsServer) {
            hitCharacter.GetComponent<BuffController>().ApplyBuffServerRpc("Stunned");

            if (!hasGainedShieldSlamShield && buffController.HasBuff("Valiant")) {
                hasGainedShieldSlamShield = true;

                buffController.ApplyBuffServerRpc("Shield Slam");
            }

            int damageMin = 12;
            int damageMax = 16;

            hitCharacter.TakeDamage(Random.Range(damageMin, damageMax + 1));
        }
    }

    private void CourageousLeap(int abilityIndex) {
        if (IsOwner) {
            LeapAbility leapAbility = (LeapAbility)abilities[abilityIndex];

            LandingSpot leapTarget = characterActions.PerformLeap(abilityIndex);

            characterActions.CreateTelegraph(leapTarget.safeSpot, abilityIndex, 1, leapAbility.LeapDuration);
        }

        GameObject leapLandVFX = Instantiate(courageousLeap.prefabs[0]);
        leapLandVFX.transform.position = transform.position;
        leapLandVFX.GetComponent<VisualEffect>().SetFloat("Size", courageousLeap.radius * .5f);
    }

    private void OnCourageousLeapLand(int abilityIndex) {
        if (NetworkManager.Singleton.IsServer) {
            List<Character> hitCharacters = GetCircleHitTargets(transform.position, courageousLeap.radius, faction);

            hitCharacters.ForEach(hitCharacter => {
                int damageMin = 20;
                int damageMax = 24;

                hitCharacter.TakeDamage(Random.Range(damageMin, damageMax + 1));
            });

            buffController.ApplyBuffServerRpc("Valiant");

            if (hitCharacters.Count > 0) {
                buffController.UpdateBuffDuration("Valiant", hitCharacters.Count * 0.75f);
            }
        }

        AudioManager.Instance.PlaySound(courageousLeapSFX, transform.position);

        GameObject leapLandVFX = Instantiate(courageousLeap.prefabs[0]);
        leapLandVFX.transform.position = transform.position;
        leapLandVFX.GetComponent<VisualEffect>().SetFloat("Size", courageousLeap.radius * 2);
    }

    private void GuardCast(int abilityIndex) {
    }

    private void OnGuardHit(Ability longsword, Character hitCharacter, Vector3 hitLocation) {
    }

    private void TauntCast(int abilityIndex) {
        if (NetworkManager.Singleton.IsServer) {
            List<Character> hitCharacters = GetCircleHitTargets(transform.position, taunt.radius, faction);

            hitCharacters.ForEach(hitCharacter => {
                OnTauntHit(hitCharacter);
            });

            buffController.ApplyBuffServerRpc("Valiant");

            if (hitCharacters.Count > 0) {
                buffController.UpdateBuffDuration("Valiant", hitCharacters.Count * 1f);
            }
        }

        GameObject tauntVFX = Instantiate(taunt.prefabs[0]);
        tauntVFX.transform.position = transform.position;
        tauntVFX.GetComponent<VisualEffect>().SetFloat("Size", taunt.radius * 2);

        AudioManager.Instance.PlaySound(tauntSFX, transform.position);
    }

    private void OnTauntHit(Character hitCharacter) {
        BaseAI aiController = hitCharacter.GetComponent<BaseAI>();
        if (aiController) {
            aiController.Taunt(this);
        }

        int damageMin = 4;
        int damageMax = 6;

        hitCharacter.TakeDamage(Random.Range(damageMin, damageMax + 1));
    }
}