using UnityEngine;
using System.Collections;

public class SwordGoblin : Character {
    public Ability slashObject;
    public GameObject slashEffect;

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        SetBaseStats(100, 4.5f);

        Ability slash = Instantiate(slashObject);

        slash.OnCastComplete = () => OnSlashCast(0);
        slash.OnHit = (Character hitCharacter, Vector3 hitLocation) => OnSlashHit(slash, hitCharacter, hitLocation);
        abilities.Add(slash);
    }

    private void OnSlashCast(int abilityIndex) {
        if (!IsOwner) {
            return;
        }

        characterActions.CreateSlash(unitController.GetAimDirection(), abilityIndex, 0);
    }

    private void OnSlashHit(Ability slash, Character hitCharacter, Vector3 hitLocation) {
        if (!hitCharacter) {
            return;
        }

        int damageMin = 7;
        int damageMax = 10;
        hitCharacter.TakeDamage(Random.Range(damageMin, damageMax + 1));
    }
}