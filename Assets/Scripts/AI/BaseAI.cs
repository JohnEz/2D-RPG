using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public enum AIState {
    IDLE,
    CHASING,
    FIGHTING
}

[RequireComponent(typeof(Character))]
[RequireComponent(typeof(UnitController))]
public class BaseAI : NetworkBehaviour {
    private const float MELEE_RANGE = 1.5f;
    private const float PROJECTILE_RANGE = 8f;

    private Character myCharacter;
    private UnitController myUnitController;

    private AIState myState = AIState.IDLE;

    private const float AGGRO_RANGE = 9f;
    private const float LEASH_RANGE_MOD = 3f;

    private Character targetCharacter;
    private AIMovementPathfinding aiMovement;

    [SerializeField]
    private LayerMask losLayer;

    private void Awake() {
        myCharacter = GetComponent<Character>();
        myUnitController = GetComponent<UnitController>();
        aiMovement = GetComponent<AIMovementPathfinding>();
    }

    public override void OnDestroy() {
        base.OnDestroy();
        SetTargetCharacter(null);
    }

    private void FixedUpdate() {
        // TODO, delete this script when not on the server?
        if (!NetworkManager.Singleton.IsServer) {
            return;
        }

        switch (myState) {
            case AIState.IDLE:
                IdleState();
                break;

            case AIState.CHASING:
                ChasingState();
                break;

            case AIState.FIGHTING:
                FightingState();
                break;
        }
    }

    public void OnAggro(Character targetEnemy) {
        myState = AIState.CHASING;
        SetTargetCharacter(targetEnemy);
    }

    private void HandleTargetDeath() {
        myState = AIState.IDLE;
        SetTargetCharacter(null);
    }

    private void SetTargetCharacter(Character character) {
        if (targetCharacter != null) {
            targetCharacter.GetComponent<NetworkStats>().HealthDepleted -= HandleTargetDeath;
        }

        targetCharacter = character;
        if (targetCharacter) {
            targetCharacter.GetComponent<NetworkStats>().HealthDepleted += HandleTargetDeath;
        }
    }

    private List<Character> GetEnemies() {
        // TODO can we get this from a unit manager instance?
        List<GameObject> unitObjects = new List<GameObject>(GameObject.FindGameObjectsWithTag("Unit"));

        return unitObjects
            .Select(unitObject => unitObject.GetComponent<Character>())
            .Where(character => character.faction != myCharacter.faction)
            .ToList();
    }

    private float GetDistanceToCharacter(Character target) {
        return Vector3.Distance(transform.position, target.transform.position);
    }

    private void IdleState() {
        List<Character> enemiesInRange = GetEnemies()
            .Where(enemy => GetDistanceToCharacter(enemy) <= AGGRO_RANGE)
            .ToList();

        // gets within range
        if (enemiesInRange.Count > 0) {
            OnAggro(enemiesInRange[0]);

            EnemyPod myPod = GetComponentInParent<EnemyPod>();

            if (myPod) {
                myPod.OnAggro(this, targetCharacter);
            }
            LogState();
            return;
        }
    }

    private void ChasingState() {
        if (GetDistanceToCharacter(targetCharacter) >= AGGRO_RANGE + LEASH_RANGE_MOD) {
            myState = AIState.IDLE;
            targetCharacter = null;
            aiMovement.Stop();
            LogState();
            return;
        }

        if (GetDistanceToCharacter(targetCharacter) < PROJECTILE_RANGE - .5f && HasLineOfSight()) {
            myState = AIState.FIGHTING;
            LogState();
            aiMovement.Stop();
            myUnitController.SetMovementInput(Vector2.zero);

            Ability currentAbility = myCharacter.abilities[0];

            if (currentAbility.abilityType == AbilityType.MELEE) {
                aiMovement.SetChaseTarget(targetCharacter.transform);
            }
            return;
        }

        if (!aiMovement.isChasing) {
            aiMovement.SetChaseTarget(targetCharacter.transform);
        }

        myUnitController.TurnToFaceTarget(targetCharacter.transform);
    }

    private void FightingState() {
        if (GetDistanceToCharacter(targetCharacter) > PROJECTILE_RANGE || !HasLineOfSight()) {
            myState = AIState.CHASING;
            LogState();
            return;
        }

        Ability currentAbility = myCharacter.abilities[0];

        float range = currentAbility.abilityType == AbilityType.MELEE ? MELEE_RANGE : PROJECTILE_RANGE;

        if (GetDistanceToCharacter(targetCharacter) <= range && myUnitController.CanCastAbility(myCharacter.abilities[0])) {
            myUnitController.UseAbilityOne();
        }

        myUnitController.TurnToFaceTarget(targetCharacter.transform);
        myUnitController.aimPosition.Value = GetAimLocation();
    }

    private bool HasLineOfSight() {
        if (!targetCharacter) {
            return true;
        }

        Vector3 targetDirection = (targetCharacter.transform.position - transform.position).normalized;
        float distance = Vector2.Distance(targetCharacter.transform.position, transform.position) - 0.25f;

        RaycastHit2D raycast = Physics2D.CircleCast(transform.position, .5f, targetDirection, distance, 1 << LayerMask.NameToLayer("Obstacles"));

        return raycast.collider == null;
    }

    private Vector2 GetAimLocation() {
        Vector2 targetMoveDirection = targetCharacter.GetComponent<UnitController>().GetMovementInput();
        Vector3 targetDirection = new Vector3(targetMoveDirection.x, targetMoveDirection.y, 0);

        Vector3 targetPosition = targetCharacter.transform.position;
        Vector3 targetVelocity = targetDirection * targetCharacter.MoveSpeed;

        UnitController targetUnit = targetCharacter.GetComponent<UnitController>();

        CharacterStateController characterStateController = targetCharacter.GetComponent<CharacterStateController>();

        if (characterStateController.IsCasting() && targetUnit.castingAbility) {
            targetVelocity *= targetUnit.castingAbility.speedWhileCasting;
        }

        float dist = (targetPosition - transform.position).magnitude;

        //TODO get from ability
        float PROJECTILE_SPEED = 20f;

        Vector3 targetLocation = targetPosition + (dist / PROJECTILE_SPEED) * targetVelocity;

        //Debug.DrawLine(transform.position, targetLocation);

        return targetLocation;
    }

    private void LogState() {
        //Debug.Log("Changing to state: " + myState);
    }

    private void OnPathFound() {
    }

    public void Taunt(Character taunter) {
        SetTargetCharacter(taunter);
    }
}