using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private int frameNextAttack;
    private PlayerStatus _status;
    private Animator _playerAnimator;

    [SerializeField] bool waitingForInput = true;
    [SerializeField] bool isRunning = false;
    [SerializeField] int attackPhase = 1;

    float range, force;
    int penetrate;

    void Start() {
        _status = transform.parent.GetComponent<PlayerStatus>();
        _playerAnimator = GetComponent<Animator>();

        //Assigning Event
        transform.parent.GetComponent<PlayerAction>().meleeAttackEventHandler += MeleeAttacking;
        transform.parent.GetComponent<PlayerAction>().rangedAttackEventHandler += RangedAttacking;
    }
 
    IEnumerator ResetAttackState() {
        _status.playerState = State.Idle;
        yield return new WaitForSeconds(0.25f); //small delay to let the animation do its work
        waitingForInput = true;
        attackPhase = 1;
    }

    public void StartResetCoroutine() {
        StartCoroutine(ResetAttackState());
    }

    #region Melee
    void MeleeAttacking(object sender, PlayerAction.ActionEventArgs e) {
        if(waitingForInput) {
            frameNextAttack = e.frameNextAttack;
            //for checking between attack phase
            if(isRunning)
                waitingForInput = false;
            //start the attack phase from the first phase
            else if(attackPhase == 1) {
                if(_status.playerState == State.Idle || _status.playerState == State.Move) {
                    waitingForInput = false;
                    _status.playerState = State.Attack;
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
            if(!waitingForInput)
                break;
            frameCount++;
            yield return new WaitForEndOfFrame();
        }
        isRunning = false;
        _playerAnimator.speed = 1f;
        if(!waitingForInput) {
            attackPhase++;
            _playerAnimator.Play("Attack"+attackPhase.ToString()+"_"+_status.worldState.ToString());
        }
        else
            StartResetCoroutine();
    }
    #endregion

    #region Ranged
    public void RangedAttacking(object sender, PlayerAction.ActionEventArgs e) {
        if(waitingForInput && (_status.playerState == State.Idle || _status.playerState == State.Move) && _status.bulletCount > 0) {
            force = e.rangedAttackForce;
            range = e.rangedAttackRange;
            penetrate = e.rangedAttackPenetration;
            waitingForInput = false;
            _status.playerState = State.Attack;
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
    }

    public void DetectWeaponRange() {
        transform.GetChild(1).GetComponent<AttackDetection>().HitscanCheck(range, penetrate);
        if(_status.worldState == State.Floating_Crouch || _status.worldState == State.Floating_Stand)
            transform.parent.GetComponent<PlayerMovement>().velocity.x = -transform.parent.GetComponent<Controller2D>().collision.faceDir * force;
    }
    #endregion
}
