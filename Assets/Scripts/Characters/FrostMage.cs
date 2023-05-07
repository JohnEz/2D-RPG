using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.VFX;
using Unity.Netcode;

public class FrostMage : Character {
    [SerializeField] private Ability frostBoltObject;
    private Ability frostBolt;
    [SerializeField] private Ability iceLanceObject;
    private Ability iceLance;
    [SerializeField] private Ability frostLeapObject;
    private Ability frostLeap;
    [SerializeField] private Ability iceBarrierObject;
    private Ability iceBarrier;
    [SerializeField] private Ability coldSnaptObject;
    private Ability coldSnap;

    // Move these onto the abilities somehow

    [SerializeField] private GameObject frostLeapVFX;

    [SerializeField] private AudioClip frostLeapSFX;

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        SetBaseStats(210, 4f);

        frostBolt = Instantiate(frostBoltObject);
        iceLance = Instantiate(iceLanceObject);
        frostLeap = Instantiate(frostLeapObject);
        iceBarrier = Instantiate(iceBarrierObject);
        coldSnap = Instantiate(coldSnaptObject);

        frostBolt.OnCastComplete = () => FrostBoltCast(0);
        frostBolt.OnHit = (Character hitCharacter, Vector3 hitLocation) => OnFrostBoltHit(frostBolt, hitCharacter, hitLocation);
        abilities.Add(frostBolt);

        iceLance.OnCastComplete = () => IceLanceCast(1);
        iceLance.OnHit = (Character hitCharacter, Vector3 hitLocation) => OnIceLanceHit(iceLance, hitCharacter, hitLocation);
        abilities.Add(iceLance);

        frostLeap.OnCastComplete = () => FrostLeapCast(2);
        abilities.Add(frostLeap);

        iceBarrier.OnCastComplete = () => IceBarrierCast(iceBarrier);
        abilities.Add(iceBarrier);

        coldSnap.OnCastComplete = () => ColdSnapCast(4);
        coldSnap.OnHit = (Character hitCharacter, Vector3 hitLocation) => ColdSnapHitLocation(coldSnap, hitCharacter, hitLocation);
        abilities.Add(coldSnap);
    }

    private void FrostBoltCast(int abilityIndex) {
        if (!IsOwner) {
            return;
        }

        characterActions.CreateProjectile(transform.position, unitController.GetAimDirection(), abilityIndex, 0);
    }

    private void OnFrostBoltHit(Ability frostBolt, Character hitCharacter, Vector3 hitLocation) {
        if (!hitCharacter) {
            return;
        }

        BuffController hitBuffController = hitCharacter.GetComponent<BuffController>();

        int damageMin = 13;
        int damageMax = 17;

        if (hitBuffController.HasBuff("Chill")) {
            damageMin += 2;
            damageMax += 2;
            hitBuffController.UpdateBuffDuration("Chill", .8f);
        }

        hitCharacter.TakeDamage(Random.Range(damageMin, damageMax + 1));
    }

    private void IceLanceCast(int abilityIndex) {
        if (!IsOwner) {
            return;
        }

        characterActions.CreateProjectile(transform.position, unitController.GetAimDirection(), abilityIndex, 0);
    }

    private void OnIceLanceHit(Ability iceLance, Character hitCharacter, Vector3 hitLocation) {
        if (!hitCharacter) {
            return;
        }

        BuffController hitBuffController = hitCharacter.GetComponent<BuffController>();

        int damageMin = 20;
        int damageMax = 24;

        if (hitBuffController.HasBuff("Chill")) {
            damageMin += 6;
            damageMax += 6;
        } else {
            hitBuffController.ApplyBuffServerRpc("Chill");
        }

        hitCharacter.TakeDamage(Random.Range(damageMin, damageMax + 1));
    }

    private void FrostLeapCast(int abilityIndex) {
        GameObject leapVFX = Instantiate(frostLeapVFX);
        leapVFX.transform.position = transform.position;

        AudioManager.Instance.PlaySound(frostLeapSFX, transform.position);

        LeapAbility leapAbility = (LeapAbility)abilities[abilityIndex];

        if (IsOwner) {
            LandingSpot leapTarget = characterActions.PerformLeap(abilityIndex);
        }

        if (NetworkManager.Singleton.IsServer) {
            List<Character> hitCharacters = GetCircleHitTargets(transform.position, leapAbility.radius, faction);

            hitCharacters.ForEach(hitCharacter => {
                hitCharacter.GetComponent<BuffController>().ApplyBuffServerRpc("Chill");
            });
        }
    }

    private void IceBarrierCast(Ability iceBarrier) {
        if (!IsOwner) {
            return;
        }

        Character targetCharacter = GetClosestUnitInRadius(unitController.aimPosition.Value, faction);

        BuffController targetBuffController;

        if (targetCharacter) {
            targetBuffController = targetCharacter.GetComponent<BuffController>();
        } else {
            targetBuffController = buffController;
        }

        targetBuffController.ApplyBuffServerRpc("Ice Barrier");
    }

    private void ColdSnapCast(int abilityIndex) {
        if (!IsOwner) {
            return;
        }

        characterActions.CreateTelegraph(unitController.aimPosition.Value, abilityIndex, 0, 0.8f);
    }

    // Move this to projectiles / telegraphs?
    private void ColdSnapHitLocation(Ability coldSnap, Character hitCharacter, Vector3 hitLocation) {
        List<Character> hitCharacters = GetCircleHitTargets(hitLocation, coldSnap.radius, faction);

        foreach (Character character in hitCharacters) {
            ColdSnapHitCharacter(character);
        }
    }

    private void ColdSnapHitCharacter(Character hitCharacter) {
        if (!hitCharacter) {
            return;
        }

        BuffController hitBuffController = hitCharacter.GetComponent<BuffController>();

        int damageMin = 16;
        int damageMax = 20;

        hitCharacter.TakeDamage(Random.Range(damageMin, damageMax + 1));

        if (hitBuffController.HasBuff("Chill")) {
            hitBuffController.RemoveBuffServerRpc("Chill");
            hitBuffController.ApplyBuffServerRpc("Frozen");
        } else {
            hitBuffController.ApplyBuffServerRpc("Chill");
        }
    }
}