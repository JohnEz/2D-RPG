using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : Singleton<InputHandler> {
    public Vector2 MovementVector { get; private set; }

    public Vector3 MousePosition { get; private set; }
    public Vector3 MouseWorldPosition { get; private set; }

    public bool DashPressed { get; private set; }

    public bool AttackPressed { get; private set; }
    public bool AltAttackPressed { get; private set; }
    public bool UtilityPressed { get; private set; }
    public bool UtilityTwoPressed { get; private set; }

    public bool UltimatePressed { get; private set; }

    public bool MovementUpPressed { get; private set; }
    public bool MovementDownPressed { get; private set; }
    public bool MovementLeftPressed { get; private set; }
    public bool MovementRightPressed { get; private set; }

    private void Update() {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        MovementVector = new Vector2(horizontalInput, verticalInput);

        MousePosition = Input.mousePosition;
        Vector3 rawMouseWorldPosition = Camera.main.ScreenToWorldPoint(MousePosition);
        MouseWorldPosition = new Vector3(rawMouseWorldPosition.x, rawMouseWorldPosition.y, 0);

        AttackPressed = Input.GetKey(KeyCode.Mouse0);
        AltAttackPressed = Input.GetKey(KeyCode.Mouse1);
        DashPressed = Input.GetKey(KeyCode.Space);
        UtilityPressed = Input.GetKey(KeyCode.Q);
        UtilityTwoPressed = Input.GetKey(KeyCode.E);

        UltimatePressed = Input.GetKeyDown(KeyCode.F);

        MovementUpPressed = Input.GetKeyDown(KeyCode.W);
        MovementDownPressed = Input.GetKeyDown(KeyCode.S);
        MovementLeftPressed = Input.GetKeyDown(KeyCode.A);
        MovementRightPressed = Input.GetKeyDown(KeyCode.D);
    }

    public Vector2 DirectionToMouse(Vector3 startPosition) {
        return new Vector2(MouseWorldPosition.x - startPosition.x, MouseWorldPosition.y - startPosition.y).normalized;
    }

    public float DistanceToMouse(Vector3 startPosition) {
        return new Vector2(MouseWorldPosition.x - startPosition.x, MouseWorldPosition.y - startPosition.y).magnitude;
    }
}