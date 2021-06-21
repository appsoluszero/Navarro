using System;
using UnityEngine;
using Cinemachine;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] private Transform head, body;
    [SerializeField] private GameManager _manager;
    [SerializeField] private float meleeShakeAmplitude = 2f;
    [SerializeField] private float rangedShakeAmplitude = 5f;
    [SerializeField] private float CameraFallbackTime;
    private CinemachineVirtualCamera mainCamera;
    private PlayerStatus _status;
    private DialogueHandler _dialogue;
    private PlayerAction _action;

    float shakeTimer = 0;
    bool isFallingBack = false;
    bool isMelee;

    void Start() {
        mainCamera = GetComponent<CinemachineVirtualCamera>();
        _status = transform.parent.GetComponent<PlayerStatus>();
        _dialogue = transform.parent.GetComponent<DialogueHandler>();
        _action = transform.GetComponentInParent<PlayerAction>();

        _dialogue.dialogueInteractEvent += DialogueStateCamera;
        _dialogue.dialogueUninteractEvent += GameplayStateCamera;
    }

    void Update() {
        if(shakeTimer >= CameraFallbackTime) {
            shakeTimer = 0f;
            isFallingBack = false;
        }

        if(isFallingBack) {
            shakeTimer += Time.deltaTime;
            shakeTimer = Mathf.Clamp(shakeTimer, 0f, CameraFallbackTime);
            float progress = shakeTimer / CameraFallbackTime;
            mainCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = Mathf.Lerp((isMelee) ? meleeShakeAmplitude : rangedShakeAmplitude, 0f, progress);
        }
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

    public void AttackCameraShake(bool isMelee) {
        this.isMelee = isMelee;
        mainCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = (isMelee) ? meleeShakeAmplitude : rangedShakeAmplitude;
        isFallingBack = true;
    }
}
