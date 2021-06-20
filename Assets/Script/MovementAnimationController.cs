using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementAnimationController : MonoBehaviour
{
    private Animator _playerAnimation;
    [SerializeField] private PlayerStatus _status;
    [SerializeField] private Controller2D _controller;
    
    [Header("Floating Error Compensation")]
    [SerializeField] private int targetCheckFrame = 5;
    private Transform _spriteTransform;
    private bool startFloating;
    void Start()
    {
        _playerAnimation = GetComponent<Animator>();
        _spriteTransform = GetComponent<Transform>();
        _status.healthDecreaseHandler += HurtAnimation;
    }

    void Update()
    {
        if(_status.playerState != State.Death && _status.playerState != State.Attack && _status.playerState != State.Hurt) {
            transform.localScale = new Vector3(_controller.collision.faceDir, 1f, 1f);
            if(_status.playerState == State.Rolling)
                _playerAnimation.Play("Rolling");
            else if(_status.playerState != State.Attack && _status.playerState != State.Hurt) {
                if(_status.worldState == State.Floating_Crouch || _status.worldState == State.Floating_Stand) {
                    if(!startFloating) {
                        startFloating = true;
                        StartCoroutine(floatingFrameCount());
                    } 
                } 
                else {
                    if(_status.playerState == State.Move) {
                        if(_status.worldState == State.Crouch)
                            _playerAnimation.Play("Move_Crouch"); 
                        else {
                            if(_status.worldState == State.Floating_Crouch)
                                _playerAnimation.Play("Move_Crouch");
                            else
                                _playerAnimation.Play("Move_Stand"); 
                        }  
                    }
                    else {
                        if(_status.worldState == State.Crouch)
                            _playerAnimation.Play("Idle_Crouch"); 
                        else {
                            _playerAnimation.Play("Idle_Stand");
                        }  
                    }
                }
            }
        }
    }

    IEnumerator floatingFrameCount() {
        int frameCount = 0;
        while(frameCount < targetCheckFrame) {
            if(_status.playerState == State.Attack || (_status.worldState != State.Floating_Crouch & _status.worldState != State.Floating_Stand)) {
                break;
            }
            frameCount++;
            yield return new WaitForEndOfFrame();
        }
        if(_status.playerState != State.Attack && frameCount == targetCheckFrame) {
            if(_status.worldState == State.Floating_Stand) {
                if(_controller.collision.verticalDir == 1) 
                    _playerAnimation.Play("FloatingUp_Stand");
                else if(_controller.collision.verticalDir == -1) 
                    _playerAnimation.Play("FloatingDown_Stand");
            }
            else if(_status.worldState == State.Floating_Crouch) 
                _playerAnimation.Play("FloatingUp_Crouch");
        }
        startFloating = false;
    }

    public void ClearHurtEvent() {
        GetComponent<PlayerAttack>().isHurt = false;
        GetComponent<PlayerAttack>().waitingForInput = true;
        GetComponent<PlayerAttack>().goingNextPhase = false;
        _status.playerState = State.Idle;
    }

    public void HurtAnimation(object sender, PlayerStatus.StatChangeEventArgs e) {
        GetComponent<PlayerAttack>().isHurt = true;
        _playerAnimation.Play("Hurt_Stand");
        _status.playerState = State.Hurt;
    }
}
