using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class BowGoblin : Character {
    public Ability shootObject;
    public GameObject shootProjectile;

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        SetBaseStats(100, 4f);

        Ability shoot = Instantiate(shootObject);

        shoot.OnCastComplete = () => ShootCast(0);
        shoot.OnHit = (Character hitCharacter, Vector3 hitLocation) => ShootHit(shoot, hitCharacter, hitLocation);
        abilities.Add(shoot);
    }

    private void ShootCast(int abilityIndex) {
        if (!IsOwner) {
            return;
        }

        characterActions.CreateProjectile(transform.position, unitController.GetAimDirection(), abilityIndex, 0);
    }

    private void ShootHit(Ability shoot, Character hitCharacter, Vector3 hitLocation) {
        if (!hitCharacter) {
            return;
        }

        int damageMin = 5;
        int damageMax = 8;

        hitCharacter.TakeDamage(Random.Range(damageMin, damageMax + 1));
    }
}