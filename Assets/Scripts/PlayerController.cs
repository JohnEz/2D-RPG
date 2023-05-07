using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(UnitController))]
public class PlayerController : NetworkBehaviour {
    private CharacterStateController stateController;
    private Character character;

    private UnitController unitController;

    private void Start() {
        character = GetComponent<Character>();
        stateController = GetComponent<CharacterStateController>();
        unitController = GetComponent<UnitController>();

        if (IsOwner) {
            CameraManager.Instance.SetFollowTarget(this.gameObject);
            ActionBarManager.Instance.SetCharacter(character);
        }
    }

    private void Update() {
        if (!IsOwner) {
            return;
        }

        if (InputHandler.Instance.MovementUpPressed || InputHandler.Instance.MovementDownPressed || InputHandler.Instance.MovementLeftPressed || InputHandler.Instance.MovementRightPressed) {
            // TODO removed until melee isnt cancelled
            //unitController.ManualCancelCast();
        }

        if (InputHandler.Instance.AttackPressed) {
            unitController.UseAbilityOne();
        }

        if (InputHandler.Instance.AltAttackPressed) {
            unitController.UseAbilityTwo();
        }

        if (InputHandler.Instance.DashPressed) {
            unitController.UseAbilityThree();
        }

        if (InputHandler.Instance.UtilityPressed) {
            unitController.UseAbilityFour();
        }

        if (InputHandler.Instance.UtilityTwoPressed) {
            unitController.UseAbilityFive();
        }
    }

    private void FixedUpdate() {
        if (!IsOwner) {
            return;
        }

        TurnToMouse();
        unitController.SetMovementInput(InputHandler.Instance.MovementVector.normalized);
        unitController.aimPosition.Value = new Vector2(InputHandler.Instance.MouseWorldPosition.x, InputHandler.Instance.MouseWorldPosition.y);
    }

    private void TurnToMouse() {
        unitController.FaceDirection(InputHandler.Instance.DirectionToMouse(transform.position));
    }
}