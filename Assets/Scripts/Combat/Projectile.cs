using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum ProjectileHitType {
    TARGET,
    LOCATION
}

public class Projectile : MonoBehaviour {
    // Character specific variables

    private CharacterFaction myFaction;
    private Ability.OnHitDelegate onHit;

    //

    public float speed = 20f;

    private Vector2 direction;

    private bool hasTargetLocation = false;
    private Vector3 targetLocation;

    private bool isActive = true;

    [SerializeField]
    private GameObject body;

    [SerializeField]
    private AudioClip onCreateSFX;

    [SerializeField]
    private GameObject onHitVFXPrefab;

    [SerializeField]
    private AudioClip onHitSFX;

    [SerializeField]
    private ProjectileHitType hitType;

    public void Initialise(CharacterFaction faction, Ability.OnHitDelegate abilityOnHit) {
        myFaction = faction;
        onHit = abilityOnHit;

        if (onCreateSFX) {
            AudioManager.Instance.PlaySound(onCreateSFX, transform);
        }
    }

    public void MoveInDirection(Vector3 spawnLocation, Vector2 startDirection) {
        direction = startDirection.normalized;
        transform.up = direction;
        transform.position = spawnLocation;
    }

    public void MoveToTarget(Vector3 spawnLocation, Vector3 target) {
        hasTargetLocation = true;
        targetLocation = target;
        targetLocation.z = 0;
        direction = (targetLocation - spawnLocation).normalized;
        transform.up = direction;
        transform.position = spawnLocation;
    }

    private void FixedUpdate() {
        if (!isActive) {
            return;
        }

        float step = speed * Time.fixedDeltaTime;

        if (hasTargetLocation) {
            float distance = Vector3.Distance(transform.position, targetLocation);
            if (distance < 0.0001f) {
                CreateHitEffects(targetLocation.x, targetLocation.y, targetLocation.z);
                HandleHit(null, targetLocation);
                Destroy(gameObject);
            }

            transform.position = Vector3.MoveTowards(transform.position, targetLocation, step);
        } else {
            Vector3 newPosition = transform.position + (Vector3)direction * step;

            transform.position = newPosition;
        }
    }

    public void OnTriggerEnter2D(Collider2D collision) {
        if (!isActive) {
            return;
        }

        //TODO find out why layer didnt work, unhardcode
        // ANSWER layer should be bit shifted 1 << mask
        bool hitWall = collision.gameObject.layer == 6;
        bool hitUnit = collision.gameObject.tag == "Unit";

        if (!hitUnit && !hitWall) {
            return;
        }

        Vector3 hitLocation = Vector3.zero;

        if (hitUnit) {
            Character hitCharacter = collision.gameObject.GetComponent<Character>();

            bool shouldHitTarget = !hitCharacter || myFaction != hitCharacter.faction;

            if (!shouldHitTarget) {
                return;
            }

            if (hitCharacter) {
                HandleHit(hitCharacter, collision.gameObject.transform.position);
            }
            hitLocation = collision.transform.position;
        }

        if (hitWall) {
            if (hasTargetLocation) {
                HandleHit(null, transform.position);
            }

            hitLocation = transform.position;
        }

        CreateHitEffects(hitLocation.x, hitLocation.y, hitLocation.z);

        if (body) {
            Destroy(body);
        }
        isActive = false;
    }

    public void CreateHitEffects(float x, float y, float z) {
        if (onHitVFXPrefab) {
            GameObject hitVFX = Instantiate(onHitVFXPrefab);
            hitVFX.transform.position = new Vector3(x, y, z);
            hitVFX.transform.rotation = transform.rotation;
        }

        if (onHitSFX) {
            AudioManager.Instance.PlaySound(onHitSFX, transform.position);
        }
    }

    private void HandleHit(Character hitCharacter, Vector3 position) {
        if (!NetworkManager.Singleton.IsServer) {
            return;
        }

        onHit(hitCharacter, position);
    }
}