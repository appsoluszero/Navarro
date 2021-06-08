using System;
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

    void Start() {
        _status = transform.parent.GetComponent<PlayerStatus>();
        _playerAnimator = GetComponent<Animator>();

        //Assigning Event
        transform.parent.GetComponent<PlayerAction>().attackEventHandler += Attacking;
    }

    void Attacking(object sender, PlayerAction.ActionEventArgs e) {
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

    public void ResetAttackState() {
        waitingForInput = true;
        _status.playerState = State.Idle;
        attackPhase = 1;
    }

    public void StartAttackCoroutine() {
        StartCoroutine(AttackSequence());
    }

    IEnumerator AttackSequence() {
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
    }
}
