using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;

[RequireComponent(typeof(Collider2D))]
public class Hitbox : MonoBehaviour {
    private Ability.OnHitDelegate OnHit;
    private CharacterFaction myFaction;

    [SerializeField] private float hitDelay = 0f;
    [SerializeField] private float hitDuration = 0f;

    private bool isActive;

    private List<int> hitIds;

    // TODO
    // add delay param
    // add duration param
    // add offset
    // add size

    public void Initialise(CharacterFaction faction, Ability.OnHitDelegate onHit, float duration) {
        hitIds = new List<int>();
        OnHit = onHit;
        myFaction = faction;
        hitDuration = duration;

        Invoke("EnableHits", hitDelay);
    }

    private void EnableHits() {
        isActive = true;

        if (hitDuration > 0) {
            Invoke("DisableHits", hitDuration);
        }
    }

    private void DisableHits() {
        // TODO may not need this and can just destroy
        isActive = false;
        Destroy(gameObject, 1f);
    }

    public void OnTriggerEnter2D(Collider2D collision) {
        if (!isActive) {
            return;
        }

        OnCollision(collision);
    }

    public void OnTriggerStay2D(Collider2D collision) {
        if (!isActive) {
            return;
        }

        OnCollision(collision);
    }

    private void OnCollision(Collider2D collision) {
        bool hitUnit = collision.gameObject.tag == "Unit";

        if (!hitUnit) {
            return;
        }

        int hitId = collision.gameObject.GetInstanceID();
        if (hitIds.Contains(hitId)) {
            return;
        }

        hitIds.Add(hitId);

        Character hitCharacter = collision.gameObject.GetComponent<Character>();

        bool shouldHitTarget = !hitCharacter || myFaction != hitCharacter.faction;

        if (!shouldHitTarget) {
            return;
        }

        if (hitCharacter) {
            HandleHit(hitCharacter, collision.gameObject.transform.position);
        }
    }

    private void HandleHit(Character hitCharacter, Vector3 hitPosition) {
        OnHit(hitCharacter, hitPosition);
    }
}