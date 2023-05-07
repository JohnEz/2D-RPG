using UnityEngine;
using System.Collections;
using Unity.Netcode;

[RequireComponent(typeof(Character))]
public class NetworkCharacterActions : NetworkBehaviour {
    private Character myCharacter;
    private UnitController unitController;

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();

        myCharacter = GetComponent<Character>();
        unitController = GetComponent<UnitController>();
    }

    private Ability GetAbility(int abilityIndex) {
        return myCharacter.abilities[abilityIndex];
    }

    public void CreateProjectile(Vector3 startPosition, Vector2 direction, int abilityIndex, int prefabIndex) {
        CreateProjectileServerRpc(startPosition, direction, abilityIndex, prefabIndex);
        CreateProjectileLocal(startPosition, direction, abilityIndex, prefabIndex);
    }

    [ServerRpc]
    private void CreateProjectileServerRpc(Vector3 startPosition, Vector2 direction, int abilityIndex, int prefabIndex) {
        CreateProjectileClientRpc(startPosition, direction, abilityIndex, prefabIndex);
    }

    [ClientRpc]
    private void CreateProjectileClientRpc(Vector3 startPosition, Vector2 direction, int abilityIndex, int prefabIndex) {
        if (IsOwner) {
            return;
        }

        CreateProjectileLocal(startPosition, direction, abilityIndex, prefabIndex);
    }

    private void CreateProjectileLocal(Vector3 startPosition, Vector2 direction, int abilityIndex, int prefabIndex) {
        Ability ability = GetAbility(abilityIndex);
        GameObject projectilePrefab = ability.prefabs[prefabIndex];

        GameObject projectile = Instantiate(projectilePrefab);
        Projectile projectileController = projectile.GetComponent<Projectile>();

        projectileController.Initialise(myCharacter.faction, ability.OnHit);
        projectileController.MoveInDirection(startPosition, direction);
    }

    public void CreateTelegraph(Vector3 startPosition, int abilityIndex, int prefabIndex, float impactDelay) {
        CreateTelegraphServerRpc(startPosition, abilityIndex, prefabIndex, impactDelay);
        CreateTelegraphLocal(startPosition, abilityIndex, prefabIndex, impactDelay);
    }

    [ServerRpc]
    private void CreateTelegraphServerRpc(Vector3 startPosition, int abilityIndex, int prefabIndex, float impactDelay) {
        CreateTelegraphClientRpc(startPosition, abilityIndex, prefabIndex, impactDelay);
    }

    [ClientRpc]
    private void CreateTelegraphClientRpc(Vector3 startPosition, int abilityIndex, int prefabIndex, float impactDelay) {
        if (IsOwner) {
            return;
        }

        CreateTelegraphLocal(startPosition, abilityIndex, prefabIndex, impactDelay);
    }

    private void CreateTelegraphLocal(Vector3 position, int abilityIndex, int prefabIndex, float impactDelay) {
        Ability ability = GetAbility(abilityIndex); ;
        GameObject telegraphPrefab = ability.prefabs[prefabIndex];

        GameObject telegraphObject = Instantiate(telegraphPrefab);
        telegraphObject.transform.position = position;

        Telegraph telegraph = telegraphObject.GetComponent<Telegraph>();
        telegraph.Initialise(impactDelay, ability.radius, ability.OnHit);
    }

    public void CreateSlash(Vector2 direction, int abilityIndex, int prefabIndex) {
        CreateSlashServerRpc(direction, abilityIndex, prefabIndex);
        CreateSlashLocal(direction, abilityIndex, prefabIndex);
    }

    [ServerRpc]
    private void CreateSlashServerRpc(Vector2 direction, int abilityIndex, int prefabIndex) {
        CreateSlashClientRpc(direction, abilityIndex, prefabIndex);
    }

    [ClientRpc]
    private void CreateSlashClientRpc(Vector2 direction, int abilityIndex, int prefabIndex) {
        if (IsOwner) {
            return;
        }

        CreateSlashLocal(direction, abilityIndex, prefabIndex);
    }

    private void CreateSlashLocal(Vector2 direction, int abilityIndex, int prefabIndex) {
        Ability ability = GetAbility(abilityIndex); ;
        GameObject slashPrefab = ability.prefabs[prefabIndex];

        GameObject slashGameObject = Instantiate(slashPrefab);
        slashGameObject.transform.SetParent(transform, false);
        slashGameObject.transform.up = direction;

        Slash slashController = slashGameObject.GetComponent<Slash>();

        slashController.Initialise(myCharacter.faction, ability.OnHit);
    }

    public LandingSpot PerformLeap(int abilityIndex) {
        LeapAbility leapAbility = (LeapAbility)GetAbility(abilityIndex);

        float distance = Mathf.Min(leapAbility.MaxDistance, InputHandler.Instance.DistanceToMouse(transform.position));

        LandingSpot leapTarget = LeapTarget.GetLeapLandingSpot(transform.position, distance, unitController.GetAimDirection());

        if (leapTarget.hasSafeSpot) {
            PerformLeapLocal(abilityIndex, leapTarget.safeSpot);
            PerformLeapServerRpc(abilityIndex, leapTarget.safeSpot);
        }

        return leapTarget;
    }

    [ServerRpc]
    private void PerformLeapServerRpc(int abilityIndex, Vector2 targetLocation) {
        PerformLeapClientRpc(abilityIndex, targetLocation);
    }

    [ClientRpc]
    private void PerformLeapClientRpc(int abilityIndex, Vector2 targetLocation) {
        if (IsOwner) {
            return;
        }

        PerformLeapLocal(abilityIndex, targetLocation);
    }

    private void PerformLeapLocal(int abilityIndex, Vector2 targetLocation) {
        LeapAbility leapAbility = (LeapAbility)GetAbility(abilityIndex);

        unitController.StartLeap(targetLocation, leapAbility.LeapDuration, leapAbility.LeapMoveCurve, leapAbility.LeapZCurve);
    }
}