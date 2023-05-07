using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum CharacterState {
    Idle,
    Moving,
    Dashing,
    Casting,
    Stunned,
    Dead,
    Leaping,
}

public class CharacterStateController : MonoBehaviour {
    private CharacterState _state;

    public CharacterState State {
        get { return _state; }
        set {
            if (_state == value) {
                return;
            }

            DebugState(_state, value);

            _state = value;
        }
    }

    public void Start() {
        State = CharacterState.Idle;
    }

    public bool IsIdle() {
        return State == CharacterState.Idle;
    }

    public bool IsMoving() {
        return State == CharacterState.Moving;
    }

    public bool IsDashing() {
        return State == CharacterState.Dashing;
    }

    public bool IsCasting() {
        return State == CharacterState.Casting;
    }

    public bool IsStunned() {
        return State == CharacterState.Stunned;
    }

    public bool IsDead() {
        return State == CharacterState.Dead;
    }

    public bool IsLeaping() {
        return State == CharacterState.Leaping;
    }

    private void DebugState(CharacterState previousState, CharacterState newState) {
        //print($"{gameObject.name}: {previousState} -> {newState}");
    }
}