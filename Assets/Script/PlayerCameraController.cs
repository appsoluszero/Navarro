using System;
using UnityEngine;
using Cinemachine;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] private Transform head, body;
    [SerializeField] private GameManager _manager;
    private CinemachineVirtualCamera mainCamera;
    private PlayerStatus _status;
    private DialogueHandler _dialogue;
    void Start() {
        mainCamera = GetComponent<CinemachineVirtualCamera>();
        _status = transform.parent.GetComponent<PlayerStatus>();
        _dialogue = transform.parent.GetComponent<DialogueHandler>();

        _dialogue.dialogueInteractEvent += DialogueStateCamera;
        _dialogue.dialogueUninteractEvent += GameplayStateCamera;
    }

    public void ToggleCameraCrouch() {
        if(_status.worldState == State.Crouch || _status.worldState == State.Floating_Crouch) 
            mainCamera.Follow = body;
        else if(_status.worldState == State.Stand || _status.worldState == State.Floating_Stand)
            mainCamera.Follow = head;
    }

    public void GameplayStateCamera(object sender, EventArgs e) {
        mainCamera.m_Lens.OrthographicSize = 7f;
        mainCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenX = 0.50f;
        mainCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenY = 0.50f;
        mainCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_DeadZoneWidth = 0.15f;
        mainCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_DeadZoneHeight = 0.02f;
    }

    public void DialogueStateCamera(object sender, EventArgs e) {
        mainCamera.m_Lens.OrthographicSize = 4f;
        mainCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenX = 0.6f;
        mainCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenY = 0.3f;
        mainCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_DeadZoneWidth = 0;
        mainCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_DeadZoneHeight = 0;
        mainCamera.Follow = head;
    }
}
