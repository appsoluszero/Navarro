using UnityEngine;
using Cinemachine;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] private Transform head, body;
    [SerializeField] private GameManager _manager;
    private CinemachineVirtualCamera mainCamera;
    private PlayerStatus _status;
    void Start() {
        mainCamera = GetComponent<CinemachineVirtualCamera>();
        _status = transform.parent.GetComponent<PlayerStatus>();
    }

    void Update() {
        ToggleCameraCrouch();
    }
 
    public void ToggleCameraCrouch() {
        if(_manager.currentGameState == gameState.Gameplay) {
            mainCamera.m_Lens.OrthographicSize = 7f;
            mainCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenX = 0.50f;
            mainCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenY = 0.50f;
            mainCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_DeadZoneWidth = 0.15f;
            mainCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_DeadZoneHeight = 0.02f;
            if(_status.worldState == State.Crouch || _status.worldState == State.Floating_Crouch) 
                mainCamera.Follow = body;
            else
                mainCamera.Follow = head;
        }
        else if(_manager.currentGameState == gameState.Dialogue) {
            mainCamera.m_Lens.OrthographicSize = 4f;
            mainCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenX = 0.6f;
            mainCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenY = 0.3f;
            mainCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_DeadZoneWidth = 0;
            mainCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_DeadZoneHeight = 0;
            mainCamera.Follow = head;
        }
    }
}
