using UnityEngine;
using System.Collections;
using Cinemachine;

public class CameraManager : Singleton<CameraManager> {

    [SerializeField]
    private CinemachineVirtualCamera playerCamera;

    [SerializeField]
    private Transform playerFollowTarget;

    [SerializeField]
    private GameObject player;

    private Vector3 playerFollowLocation;

    public void SetFollowTarget(GameObject target) {
        player = target;
    }

    private void Update() {
        if (!player || !playerFollowTarget) {
            return;
        }

        Vector3 mouseWorldPosition = InputHandler.Instance.MouseWorldPosition;
        mouseWorldPosition.z = 0;

        playerFollowLocation = (mouseWorldPosition + player.transform.position) / 2;
        playerFollowLocation = (playerFollowLocation + player.transform.position) / 2;

        playerFollowTarget.position = playerFollowLocation;
    }
}