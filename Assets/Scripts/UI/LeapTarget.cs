using UnityEngine;
using System.Collections;

public struct LandingSpot {
    public bool hasSafeSpot;
    public Vector3 safeSpot;
}

public class LeapTarget : MonoBehaviour {
    public Transform StartTransform { get; set; }
    public float MaxDistance { get; set; }

    public static LandingSpot GetLeapLandingSpot(Vector3 startLocation, float maxDistance, Vector3 direction) {
        float playerWidth = .5f;

        Vector3 endPoint = startLocation + (direction * maxDistance);
        Vector3 reverseDir = direction * -1;

        float checkInterval = 0.2f;
        float currentDistance = 0;
        LandingSpot landingSpot = new LandingSpot();
        landingSpot.hasSafeSpot = false;

        while (!landingSpot.hasSafeSpot && currentDistance < maxDistance) {
            Vector3 positionToCheck = endPoint + reverseDir * currentDistance;

            Collider2D collider = Physics2D.OverlapCircle(positionToCheck, playerWidth, 1 << LayerMask.NameToLayer("Obstacles"));

            if (!collider) {
                landingSpot.hasSafeSpot = true;
                landingSpot.safeSpot = positionToCheck;
                continue;
            }

            currentDistance += checkInterval;
        }

        return landingSpot;
    }

    private void Update() {
        if (!StartTransform) {
            return;
        }

        float distance = Mathf.Min(MaxDistance, InputHandler.Instance.DistanceToMouse(StartTransform.position));

        LandingSpot landingSpot = GetLeapLandingSpot(StartTransform.position, distance, InputHandler.Instance.DirectionToMouse(StartTransform.position));

        if (landingSpot.hasSafeSpot) {
            transform.position = landingSpot.safeSpot;
        }
    }
}