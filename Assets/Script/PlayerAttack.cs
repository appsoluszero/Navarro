using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private int frameIntervalBetweenAttack = 20;
    private PlayerStatus _status;
    private Controller2D _controller;
    private Animator _playerAnimator;

    [SerializeField] bool waitingForInput = true;
    [SerializeField] bool isRunning = false;
    [SerializeField] int attackPhase = 1;

    void Start() {
        _status = transform.parent.GetComponent<PlayerStatus>();
        _controller = transform.parent.GetComponent<Controller2D>();
        _playerAnimator = GetComponent<Animator>();
    }

    void Update() {
        if(Input.GetKeyDown(InputManager.actionsMap["attack"]) && waitingForInput) 
            Attacking();
    }

    void Attacking() {
        //for checking between attack phase
        if(isRunning)
            waitingForInput = false;
        //start the attack phase from the first phase
        if(attackPhase == 1) {
            if(_status.playerState == State.Idle || _status.playerState == State.Move) {
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
        while(frameCount < frameIntervalBetweenAttack) {
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
