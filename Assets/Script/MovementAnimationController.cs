using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementAnimationController : MonoBehaviour
{
    private Animator _playerAnimation;
    private PlayerStatus _status;
    private Controller2D _controller;
    private PlayerAction _action;
    private PlayerAttack _attack;
    [Header("Rolling Animation")]
    [SerializeField] private Sprite[] rollingSprite;
    [SerializeField] private int rollingFrame = 12;
    
    [Header("Floating Error Compensation")]
    [SerializeField] private int targetCheckFrame = 5;
    private SpriteRenderer _spriteRenderer;
    private bool startFloating;
    void Start()
    {
        _playerAnimation = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _action = transform.parent.GetComponent<PlayerAction>();
        _status = transform.parent.GetComponent<PlayerStatus>();
        _controller = transform.parent.GetComponent<Controller2D>();
        _attack = GetComponent<PlayerAttack>();
        _status.healthDecreaseHandler += HurtAnimation;
        _action.rollEventHandler += RollAnimation;
    }

    void Update()
    {
        if(_status.playerState != State.Death && _status.playerState != State.MeleeAttack && _status.playerState != State.RangedAttack && _status.playerState != State.Hurt) {
            transform.localScale = new Vector3(_controller.collision.faceDir, 1f, 1f);
            if(_status.playerState != State.Rolling) {
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
            if(_status.playerState == State.MeleeAttack || _status.playerState == State.RangedAttack || (_status.worldState != State.Floating_Crouch & _status.worldState != State.Floating_Stand)) {
                break;
            }
            frameCount++;
            yield return new WaitForEndOfFrame();
        }
        if(_status.playerState != State.MeleeAttack && _status.playerState != State.RangedAttack && _status.playerState != State.Hurt && frameCount == targetCheckFrame) {
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
        _attack.isHurt = false;
        _attack.waitingForInput = true;
        _attack.goingNextPhase = false;
        _status.playerState = State.Idle;
    }

    public void HurtAnimation(object sender, PlayerStatus.StatChangeEventArgs e) {
        _attack.isHurt = true;
        _playerAnimation.Play("Hurt_Stand");
        _status.playerState = State.Hurt;
        StopCoroutine(floatingFrameCount());
    }

    public void RollAnimation(object sender, PlayerAction.ActionEventArgs e) {
        _playerAnimation.enabled = false;
        StartCoroutine(RollAnimationSequence(e.timeRoll));
    }

    IEnumerator RollAnimationSequence(float time) {
        float timePerFrame = time / (float)rollingFrame;
        int currentFrame = 0;
        while(currentFrame < rollingSprite.Length) {
            _spriteRenderer.sprite = rollingSprite[currentFrame];
            if(currentFrame == 1 || currentFrame == 2)
                yield return new WaitForSeconds(timePerFrame * 4);
            else
                yield return new WaitForSeconds(timePerFrame);
            currentFrame++;
        }
        _playerAnimation.enabled = true;
    }
}
