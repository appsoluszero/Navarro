using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private int frameNextAttack;
    private PlayerStatus _status;
    private PlayerAction _action;
    private PlayerMovement _movement;
    private Controller2D _controller;
    private AttackDetection _rangedDetection;
    private PlayerCameraController _camController;
    private Animator _playerAnimator;

    public bool waitingForInput = true;
    [HideInInspector] public bool goingNextPhase = false;
    bool isRunning = false;
    [SerializeField] int attackPhase = 1;

    float range, force;
    int penetrate;
    public bool isHurt;
    [SerializeField] private float delayBetweenAttack;

    void Start() {
        _status = transform.GetComponentInParent<PlayerStatus>();
        _action = transform.GetComponentInParent<PlayerAction>();
        _movement = transform.GetComponentInParent<PlayerMovement>();
        _rangedDetection = transform.GetChild(1).GetComponent<AttackDetection>();
        _controller = transform.GetComponentInParent<Controller2D>();
        _playerAnimator = GetComponent<Animator>();
        _camController = transform.parent.GetChild(0).GetComponent<PlayerCameraController>();

        //Assigning Event
        _action.meleeAttackEventHandler += MeleeAttacking;
        _action.rangedAttackEventHandler += RangedAttacking;
    }
 
    IEnumerator ResetAttackState() {
        _status.playerState = State.Idle;
        yield return new WaitForSeconds(delayBetweenAttack); //small delay to let the animation do its work
        waitingForInput = true;
        attackPhase = 1;
    }

    public void StartResetCoroutine() {
        StartCoroutine(ResetAttackState());
    }

    #region Melee
    void MeleeAttacking(object sender, PlayerAction.ActionEventArgs e) {
        frameNextAttack = e.frameNextAttack;
        if(isRunning)
            goingNextPhase = true;
        else if(attackPhase == 1) {
            if(_status.playerState == State.Idle || _status.playerState == State.Move) {
                waitingForInput = false;
                _status.playerState = State.MeleeAttack;
                if(_status.worldState == State.Stand)
                    _playerAnimator.Play("Attack1_Stand");
                else if(_status.worldState == State.Crouch)
                    _playerAnimator.Play("Attack1_Crouch");
                else if(_status.worldState == State.Floating_Stand)
                    _playerAnimator.Play("Attack_Stand");
                else
                    _playerAnimator.Play("Attack_Crouch");
            }  
        } 
    }

    public void StartAttackCoroutine() {
        StartCoroutine(MeleeAttackSequence());
    }

    IEnumerator MeleeAttackSequence() {
        _playerAnimator.speed = 0f;
        waitingForInput = true;
        isRunning = true;
        int frameCount = 0;
        while(frameCount < frameNextAttack) {
            if(isHurt) {
                isRunning = false;
                _playerAnimator.speed = 1f;
                StopAllCoroutines();
            }  
            if(goingNextPhase)
                break;
            frameCount++;
            yield return new WaitForEndOfFrame();
        }
        waitingForInput = false;
        isRunning = false;
        _playerAnimator.speed = 1f;
        if(goingNextPhase) {
            goingNextPhase = false;
            attackPhase++;
            _playerAnimator.Play("Attack"+attackPhase.ToString()+"_"+_status.worldState.ToString());
        }
        else
            StartResetCoroutine();
    }
    #endregion

    #region Ranged
    public void RangedAttacking(object sender, PlayerAction.ActionEventArgs e) {
        force = e.rangedAttackForce;
        range = e.rangedAttackRange;
        penetrate = e.rangedAttackPenetration;
        waitingForInput = false;
        _status.playerState = State.RangedAttack;
        _status.DecreaseBullet();
        if(_status.worldState == State.Stand) 
            _playerAnimator.Play("RangedAttack_Stand");
        else if(_status.worldState == State.Crouch)
            _playerAnimator.Play("RangedAttack_Crouch");
        else if(_status.worldState == State.Floating_Stand) 
            _playerAnimator.Play("RangedAttack_FloatStand");
        else 
            _playerAnimator.Play("RangedAttack_FloatCrouch");
    }

    public void DetectWeaponRange() {
        _rangedDetection.HitscanCheck(range, penetrate);
        _camController.AttackCameraShake(false);
        if(_status.worldState == State.Floating_Crouch || _status.worldState == State.Floating_Stand)
            _movement.velocity.x = -_controller.collision.faceDir * force;
    }
    #endregion
}
