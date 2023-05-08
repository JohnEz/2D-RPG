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

        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            // TODO pull this out into a function and maybe refactor the unit controller function to just take the ability again
            var ability = unitController.GetAbility(0);
            if (ability.IsOnCooldown()) {
                GUIManager.Instance.CreateCooldownText(ability.Icon, ability.GetCooldownAsString());
            }
        }

        if (InputHandler.Instance.AttackPressed) {
            unitController.UseAbilityOne();
        }

        if (Input.GetKeyDown(KeyCode.Mouse1)) {
            var ability = unitController.GetAbility(1);
            if (ability.IsOnCooldown()) {
                GUIManager.Instance.CreateCooldownText(ability.Icon, ability.GetCooldownAsString());
            }
        }

        if (InputHandler.Instance.AltAttackPressed) {
            unitController.UseAbilityTwo();
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            var ability = unitController.GetAbility(2);
            if (ability.IsOnCooldown()) {
                GUIManager.Instance.CreateCooldownText(ability.Icon, ability.GetCooldownAsString());
            }
        }

        if (InputHandler.Instance.DashPressed) {
            unitController.UseAbilityThree();
        }

        if (Input.GetKeyDown(KeyCode.Q)) {
            var ability = unitController.GetAbility(3);
            if (ability.IsOnCooldown()) {
                GUIManager.Instance.CreateCooldownText(ability.Icon, ability.GetCooldownAsString());
            }
        }

        if (InputHandler.Instance.UtilityPressed) {
            unitController.UseAbilityFour();
        }

        if (Input.GetKeyDown(KeyCode.E)) {
            var ability = unitController.GetAbility(4);
            if (ability.IsOnCooldown()) {
                GUIManager.Instance.CreateCooldownText(ability.Icon, ability.GetCooldownAsString());
            }
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