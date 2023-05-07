using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;

public class Slash : MonoBehaviour {
    private Ability.OnHitDelegate OnHit;
    private CharacterFaction myFaction;

    [SerializeField] private float hitDelay = 0.1f;
    [SerializeField] private float hitDuration = 0.2f;

    private bool isActive;

    [SerializeField] private Collider2D hitBox;

    private List<int> hitIds;

    public void Initialise(CharacterFaction faction, Ability.OnHitDelegate onHit) {
        hitIds = new List<int>();
        OnHit = onHit;
        myFaction = faction;

        Invoke("EnableHits", hitDelay);
    }

    private void EnableHits() {
        isActive = true;
        Invoke("DisableHits", hitDuration);
    }

    private void DisableHits() {
        isActive = false;
        Destroy(gameObject, 1f);
    }

    public void OnTriggerStay2D(Collider2D collision) {
        if (!isActive) {
            return;
        }

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
        // ADD hit VFX
        //CreateHitEffect(hitPosition);

        if (NetworkManager.Singleton.IsServer) {
            OnHit(hitCharacter, hitPosition);
        }
    }
}