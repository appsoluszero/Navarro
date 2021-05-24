using UnityEngine;
using Cinemachine;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] private Transform head, body;
    private CinemachineVirtualCamera mainCamera;
    [SerializeField] private Controller2D _controller;
    void Start() {
        mainCamera = GetComponent<CinemachineVirtualCamera>();
    }

    public void ToggleCameraCrouch() {
        if(_controller.collision.crouching) 
            mainCamera.Follow = body;
        else
            mainCamera.Follow = head;
    }
}
