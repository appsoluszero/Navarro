using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAction : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private GameManager _manager;
    [Header("Jumping Parameters")]
    public float jumpHeight = 4;
    public float timeToMaxHeight = 0.4f;

    [Header("Rolling Parameter")]
    [SerializeField] private float rollingSpeed = 7f;
    public float timeToFinishRoll = 0.5f;
    [SerializeField] private float staminaUsage = 20;

    [Header("Crouching Parameters")]
    [SerializeField] private float crouchSpeedMultiplier = 0.5f; 
    [Header("Melee Attack Parameters")]
    [SerializeField] private int frameToNextAttack = 20;
    [SerializeField] private float meleeDamage = 25f;
    [Header("Ranged Attack Parameters")]
    [SerializeField] private float maxAttackRange = 25f;
    [SerializeField] private int maxPenetration = 3;
    [SerializeField] private float forceRangedAttack = 20f;
    [SerializeField] private float rangedDamage = 65f;

    //Reference
    private PlayerMovement _movement;
    private PlayerAttack _attack;
    private Controller2D _controller;
    private PlayerStatus _status;
    private BoxCollider2D _collider;
    private BoxCollider2D _hitbox;
    private PlayerCameraController _camController;

    public event EventHandler jumpEventHandler;
    public event EventHandler<ActionEventArgs> rollEventHandler;
    public event EventHandler<ActionEventArgs> crouchEventHandler;
    public event EventHandler dropDownEventHandler;
    public event EventHandler<ActionEventArgs> meleeAttackEventHandler;
    public event EventHandler<ActionEventArgs> rangedAttackEventHandler;
    public class ActionEventArgs : EventArgs {
        public float rollSpd;
        public float timeRoll;
        public float stamUsage;
        public State thisCrouchAction;
        public bool forceUncrouch;
        public float crouchSpdMul;
        public float rangedAttackRange;
        public float rangedAttackForce;
        public int rangedAttackPenetration;
        public int frameNextAttack;
        public float meleeAttackDamage;
        public float rangedAttackDamage;
    }

    public event EventHandler<PlayerStatus.StatChangeEventArgs> staminaHandler;

    void Start() {
        //initialize
        _collider = GetComponent<BoxCollider2D>();
        //_hitbox = transform.GetChild(2).GetChild(0).GetComponent<BoxCollider2D>();
        _movement = GetComponent<PlayerMovement>();
        _controller = GetComponent<Controller2D>();
        _status = GetComponent<PlayerStatus>();
        _camController = transform.GetChild(0).GetComponent<PlayerCameraController>();
        _attack = GetComponentInChildren<PlayerAttack>();

        //Assigning Event
        jumpEventHandler += Jumping;
        rollEventHandler += RollingDodge;
        crouchEventHandler += CrouchToggle;
        dropDownEventHandler += DropDownPlatform;
    }

    void Update() {
        if(_manager.currentGameState == gameState.Gameplay) {
            if(_status.playerState != State.Hurt) {
                if(Input.GetKeyDown(InputManager.actionsMap["attack"])) {
                    if(_attack.waitingForInput && _status.playerState != State.RangedAttack) 
                        meleeAttackEventHandler?.Invoke(this, new ActionEventArgs {
                            meleeAttackDamage = meleeDamage,
                            frameNextAttack = frameToNextAttack
                        });
                } 
                else if(Input.GetKeyDown(InputManager.actionsMap["attackRanged"])) {
                    if(_attack.waitingForInput && _status.bulletCount > 0 && _status.playerState != State.MeleeAttack) 
                        rangedAttackEventHandler?.Invoke(this, new ActionEventArgs {
                            rangedAttackDamage = rangedDamage,
                            rangedAttackRange = maxAttackRange,
                            rangedAttackForce = forceRangedAttack,
                            rangedAttackPenetration = maxPenetration
                        });
                } 
                else {
                    if(_status.playerState == State.Idle || _status.playerState == State.Move) {
                        if(Input.GetKeyDown(InputManager.actionsMap["rollingDodge"])) {
                            if(_controller.collision.below && _status.currentStamina - staminaUsage >= 0) 
                                rollEventHandler?.Invoke(this, new ActionEventArgs {
                                rollSpd = rollingSpeed,
                                timeRoll = timeToFinishRoll,
                                stamUsage = staminaUsage,
                                crouchSpdMul = crouchSpeedMultiplier
                            });
                        }   
                        else if(Input.GetKeyDown(InputManager.actionsMap["jumpingUp"])) 
                            jumpEventHandler?.Invoke(this, EventArgs.Empty);
                        /*else if(Input.GetKeyDown(InputManager.actionsMap["crouchDown"]))
                            crouchEventHandler?.Invoke(this, new ActionEventArgs {
                                crouchSpdMul = crouchSpeedMultiplier
                            });*/
                    }
                    if(_status.playerState != State.MeleeAttack && _status.playerState != State.RangedAttack) {
                        if(Input.GetKey(InputManager.actionsMap["dropDown"])) {
                            if(_controller.collision.below && _controller.hitPlatform)
                                dropDownEventHandler?.Invoke(this, EventArgs.Empty);
                        } 
                    }         
                }
            }
        }
    }

    void RollingDodge(object sender, PlayerAction.ActionEventArgs e) {
        _status.playerState = State.Rolling;
        Crouching(e.crouchSpdMul);
        StartCoroutine(rollingSequence(e.rollSpd, e.stamUsage, e.timeRoll));
    }

    IEnumerator rollingSequence(float rollSpd, float stamUsage, float timeRoll) {
        _movement.velocity.x = rollSpd * _controller.collision.faceDir;
        staminaHandler?.Invoke(this, new PlayerStatus.StatChangeEventArgs { staminaUse = stamUsage });
        yield return new WaitForSeconds(timeRoll);
        Uncrouching();
        _movement.velocity.x = 0f;
        _status.playerState = State.Idle;
    }

    void Jumping(object sender, EventArgs e) {
        if(_controller.collision.below) {
            _movement.velocity.y = _movement.jumpVelocity;
            Uncrouching();
        }
    }

    void CrouchToggle(object sender, PlayerAction.ActionEventArgs e) {
        if(_status.worldState == State.Stand) 
            Crouching(e.crouchSpdMul);
        else if(_status.worldState == State.Crouch) 
            Uncrouching();
    }

    void Crouching(float spdMul) {
        _status.worldState = State.Crouch;
        _collider.offset = new Vector2(0, 0);
        _collider.size = new Vector2(0.50f, 1.125f);
        //_hitbox.offset = new Vector2(0, 0);
        //_hitbox.size = new Vector2(1, 1);
        _controller.CalculateRaySpacing();
        _camController.ToggleCameraCrouch();
        _movement.crouchMultiplier = spdMul;
    }

    void Uncrouching() {
        RaycastHit2D hit1 = Physics2D.Raycast(_controller.raycastOriginPos.topRight, Vector2.up, 1.125f + _controller.skinWidth, _controller.levelMask);
        RaycastHit2D hit2 = Physics2D.Raycast(_controller.raycastOriginPos.topLeft, Vector2.up, 1.125f + _controller.skinWidth, _controller.levelMask);
        if(!hit1 && !hit2) {
            _status.worldState = State.Stand;
            _collider.offset = new Vector2(0, 0.5625f);
            _collider.size = new Vector2(0.50f, 2.25f);
            //_hitbox.offset = new Vector2(0, 0.5f);
            //_hitbox.size = new Vector2(1, 2);
            _controller.CalculateRaySpacing();
            _camController.ToggleCameraCrouch();
            _movement.crouchMultiplier = 1f;
        }
    }

    void DropDownPlatform(object sender, EventArgs e) {
        _controller.collision.droppingDown = true;
        if(_status.playerState != State.Rolling)
            Uncrouching();
    }
}
