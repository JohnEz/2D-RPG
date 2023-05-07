using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.VFX;
using UnityEngine.Events;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Character))]
[RequireComponent(typeof(CharacterStateController))]
public class UnitController : NetworkBehaviour {
    private Character myCharacter;

    [SerializeField]
    private GameObject visuals;

    private Rigidbody2D body;
    private CharacterStateController stateController;
    private Collider2D hitbox;

    [SerializeField]
    private AnimationCurve dashSpeedCurve;

    [SerializeField]
    private float dashSpeed = 20f;

    private float timeDashing;
    private float dashDuration = .5f;
    private Vector2 dashDirection = Vector2.up;

    private Vector2 movementInput;
    public NetworkVariable<Vector2> aimPosition = new NetworkVariable<Vector2>(Vector2.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public Ability castingAbility;
    private bool castRequest, castSuccess;
    private float castStartTime = 0;
    private float castTime = 0;

    private GameObject castVFX;
    public UnityEvent<Ability> OnCastStart = new UnityEvent<Ability>();
    public UnityEvent OnCastFail = new UnityEvent();
    public UnityEvent OnCastSuccess = new UnityEvent();

    private void Awake() {
        myCharacter = GetComponent<Character>();
        body = GetComponent<Rigidbody2D>();
        stateController = GetComponent<CharacterStateController>();
        hitbox = GetComponent<Collider2D>();
    }

    private void Update() {
    }

    private void FixedUpdate() {
        if (stateController.IsDashing()) {
            DashingUpdate();
        }

        if (stateController.IsStunned()) {
            return;
        }

        if (!stateController.IsDashing()) {
            MoveFromInput();
        }

        if (!stateController.IsDashing() && !stateController.IsCasting() && !stateController.IsLeaping()) {
            stateController.State = CharacterState.Idle;
        }
    }

    // TODO move to a get set
    public void SetMovementInput(Vector2 input) {
        movementInput = input.normalized;
    }

    public Vector2 GetMovementInput() {
        return movementInput;
    }

    public Vector2 GetAimDirection() {
        return new Vector2(aimPosition.Value.x - transform.position.x, aimPosition.Value.y - transform.position.y).normalized;
    }

    public void MoveTowardsTarget(Transform targetTransform) {
        transform.position = Vector3.MoveTowards(transform.position, targetTransform.position, myCharacter.MoveSpeed * Time.deltaTime);
    }

    public void TurnToFaceTarget(Transform targetTransform) {
        Vector3 directionToTarget = (targetTransform.transform.position - transform.position).normalized;
        FaceDirection(directionToTarget);
    }

    public void FaceDirection(Vector3 direction) {
        visuals.transform.up = direction;
    }

    private void MoveFromInput() {
        if (movementInput.magnitude == 0) {
            return;
        }

        float moveSpeed = stateController.IsCasting() ? myCharacter.MoveSpeed * castingAbility.speedWhileCasting : myCharacter.MoveSpeed;

        Vector3 newPosition = body.position + (movementInput * moveSpeed) * Time.fixedDeltaTime;

        body.MovePosition(newPosition);
    }

    public bool CanCastAbility(Ability abilityToCast) {
        return !stateController.IsDashing() && !stateController.IsCasting() && abilityToCast.CanCast() && !stateController.IsStunned() && !stateController.IsLeaping();
    }

    public void CastAbility(int abilityIndex) {
        if (!CanCastAbility(myCharacter.abilities[abilityIndex])) {
            return;
        }

        CastServerRPC(abilityIndex);
        Cast(myCharacter.abilities[abilityIndex]);
    }

    public void UseAbilityOne() {
        CastAbility(0);
    }

    public void UseAbilityTwo() {
        CastAbility(1);
    }

    public void UseAbilityThree() {
        CastAbility(2);
    }

    public void UseAbilityFour() {
        CastAbility(3);
    }

    public void UseAbilityFive() {
        CastAbility(4);
    }

    // DASHING
    ///////////
    public void StartDashing(Vector3 direction, AnimationCurve speedCurve, float duration, float speed) {
        // Not sure this is required since its on cast now
        //if (stateController.IsDashing() || stateController.IsCasting() || stateController.IsStunned() || stateController.IsLeaping()) {
        //    return;
        //}

        stateController.State = CharacterState.Dashing;
        timeDashing = 0f;

        dashDirection = new Vector2(direction.x, direction.y);
        dashDuration = duration;
        dashSpeed = speed;
        dashSpeedCurve = speedCurve;
    }

    private void DashingUpdate() {
        timeDashing += Time.fixedDeltaTime;
        float currentCurveSpeed = dashSpeedCurve.Evaluate(timeDashing / dashDuration);
        Vector3 newPosition = body.position + (dashDirection * currentCurveSpeed * dashSpeed) * Time.fixedDeltaTime;

        body.MovePosition(newPosition);

        if (timeDashing >= dashDuration) {
            EndDashing();
        }
    }

    private void EndDashing() {
        if (!stateController.IsDashing()) {
            Debug.Log("EndDashing called when wasnt in dashing state");
            return;
        }

        stateController.State = CharacterState.Idle;
    }

    // LEAP
    //////////

    public void StartLeap(Vector2 leapTarget, float leapDuration, AnimationCurve leapMoveCurve, AnimationCurve leapZCurve) {
        //TODO this probably needs relaying to the player somehow?
        //if (!leapTarget.hasSafeSpot) {
        //    print("There was no safe spot");
        //    return;
        //}

        stateController.State = CharacterState.Leaping;

        if (IsOwner) {
            transform.DOLocalMoveY(leapTarget.y, leapDuration).SetEase(leapMoveCurve);
            transform.DOLocalMoveX(leapTarget.x, leapDuration).SetEase(leapMoveCurve);
        }

        transform.DOScale(1.75f, leapDuration).SetEase(leapZCurve).OnComplete(LeapComplete);

        GetComponent<Rigidbody2D>().isKinematic = true;
        hitbox.enabled = false;
    }

    // need to make this an rpc?
    private void LeapComplete() {
        stateController.State = CharacterState.Idle;
        GetComponent<Rigidbody2D>().isKinematic = false;
        hitbox.enabled = true;
        castingAbility.OnComplete();
    }

    // CASTING
    ////////////

    [ServerRpc]
    private void CastServerRPC(int abilityIndex) {
        CastClientRPC(abilityIndex);
    }

    [ClientRpc]
    private void CastClientRPC(int abilityIndex) {
        if (IsOwner) {
            return;
        }
        print("client rpc cast abilityIndex");
        Cast(myCharacter.abilities[abilityIndex]);
    }

    public void Cast(Ability ability) {
        castTime = ability.castTime;
        castingAbility = ability;
        StartCoroutine(Casting());
        OnCastStart.Invoke(ability);
    }

    private IEnumerator Casting() {
        stateController.State = CharacterState.Casting;

        RequestCast();

        List<GameObject> targetGraphics = new List<GameObject>();

        if (IsOwner && castingAbility.targetGraphics.Count > 0) {
            castingAbility.targetGraphics.ForEach(targetGraphic => {
                // TODO move this out into targetGraphicController
                GameObject targetGraphicObject = Instantiate(targetGraphic.prefab);
                TargetGraphicController targetGraphicController = targetGraphicObject.GetComponent<TargetGraphicController>();

                switch (targetGraphic.myStyle) {
                    case TargetGraphicStyle.SELF:
                        targetGraphicController.InitialiseSelfTarget(transform, targetGraphic.scale);
                        break;

                    case TargetGraphicStyle.FOLLOW_MOUSE:
                        targetGraphicController.InitialiseFollowMouseTarget(targetGraphic.scale);
                        break;

                    case TargetGraphicStyle.LEAP:
                        targetGraphicController.InitialiseLeapTarget(transform, 10f, targetGraphic.scale);
                        break;
                }

                targetGraphics.Add(targetGraphicObject);
            });
        }

        if (castingAbility.castVFX) {
            castVFX = Instantiate(castingAbility.castVFX, visuals.transform);
            castVFX.GetComponent<VisualEffect>().SetFloat("Duration", castingAbility.castTime);
        }

        if (castingAbility.castSFX) {
            AudioManager.Instance.PlaySound(castingAbility.castSFX, visuals.transform);
        }

        yield return new WaitUntil(() => castRequest == false);

        // TODO this is grim
        if (!stateController.IsStunned()) {
            stateController.State = CharacterState.Idle;
        }

        targetGraphics
            .ForEach(targetObject => Destroy(targetObject));

        if (castVFX) {
            Destroy(castVFX);
        }

        if (castSuccess) {
            castingAbility.timeCast = Time.time;

            castingAbility.OnCastComplete();
        }
    }

    public void ManualCancelCast() {
        CancelCast();
        CancelCastServerRpc(true);
    }

    [ServerRpc]
    public void CancelCastServerRpc(bool manualCancel) {
        CancelCast();
        CancelCastClientRpc(manualCancel);
    }

    [ClientRpc]
    private void CancelCastClientRpc(bool manualCancel) {
        if (IsOwner && manualCancel) {
            return;
        }

        CancelCast();
    }

    private void CancelCast() {
        if (!stateController.IsCasting()) {
            return;
        }

        CastFail();
    }

    private void RequestCast() {
        castRequest = true;
        castSuccess = false;
        castStartTime = Time.time;
        Invoke("CastSuccess", castTime);
    }

    private void CastSuccess() {
        castRequest = false;
        castSuccess = true;
        OnCastSuccess.Invoke();
    }

    public void CastFail() {
        castRequest = false;
        castSuccess = false;
        CancelInvoke("CastSuccess");
        OnCastFail.Invoke();
    }
}