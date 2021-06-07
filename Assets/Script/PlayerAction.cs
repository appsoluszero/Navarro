using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAction : MonoBehaviour
{
    [Header("Jumping Parameters")]
    public float jumpHeight = 4;
    public float timeToMaxHeight = 0.4f;

    [Header("Rolling Parameter")]
    [SerializeField] private float rollingSpeed = 7f;
    [SerializeField] private float timeToFinishRoll = 0.5f;
    [SerializeField] private float staminaUsage = 20;

     [Header("Crouching Parameters")]
    [SerializeField] private float crouchSpeedMultiplier = 0.5f; 

    //Reference
    private PlayerMovement _movement;
    private Controller2D _controller;
    private PlayerStatus _status;
    private BoxCollider2D _collider;
    private BoxCollider2D _hitbox;
    private PlayerCameraController _camController;

    public event EventHandler<PlayerStatus.StatDecreaseEventArgs> staminaBarHandler;

    void Start() {
        //initialize
        _collider = GetComponent<BoxCollider2D>();
        _hitbox = transform.GetChild(1).GetComponent<BoxCollider2D>();
        _movement = GetComponent<PlayerMovement>();
        _controller = GetComponent<Controller2D>();
        _status = GetComponent<PlayerStatus>();
        _camController = transform.GetChild(0).GetComponent<PlayerCameraController>();
    }

    void Update() {
        if(_status.playerState == State.Idle || _status.playerState == State.Move) {
            if(Input.GetKeyDown(InputManager.actionsMap["rollingDodge"])) 
                RollingDodge();
            else if(Input.GetKeyDown(InputManager.actionsMap["jumpingUp"])) 
                Jumping();
            else if(Input.GetKeyDown(InputManager.actionsMap["crouchDown"])) {
                CrouchToggle();
            }
        }
        if(_status.playerState != State.Attack) 
            if(Input.GetKey(InputManager.actionsMap["dropDown"])) 
                DropDownPlatform();
    }

    void Jumping() {
        if(_controller.collision.below) {
            _movement.velocity.y = _movement.jumpVelocity;
            Uncrouching(false);
        }
    }

    void RollingDodge() {
        if(_controller.collision.below) {
            _status.playerState = State.Rolling;
            Crouching(true);
            StartCoroutine(rollingSequence());
        }
    }

    IEnumerator rollingSequence() {
        _movement.velocity.x = rollingSpeed * _controller.collision.faceDir;
        staminaBarHandler?.Invoke(this, new PlayerStatus.StatDecreaseEventArgs { staminaUse = staminaUsage });
        yield return new WaitForSeconds(timeToFinishRoll);
        Uncrouching(true);
        //_movement.velocity.x = 0f;
        _status.playerState = State.Idle;
    }

    void CrouchToggle() {
        if(_status.worldState == State.Stand) 
            Crouching(false);
        else if(_status.worldState == State.Crouch) 
            Uncrouching(false);
    }

    void Crouching(bool ignoreSpeedChange) {
        _status.worldState = State.Crouch;
        _collider.offset = new Vector2(0, 0);
        _collider.size = new Vector2(0.50f, 1.125f);
        _hitbox.offset = new Vector2(0, 0);
        _hitbox.size = new Vector2(1, 1);
        _controller.CalculateRaySpacing();
        _camController.ToggleCameraCrouch();
        if(!ignoreSpeedChange)
            _movement.crouchMultiplier = crouchSpeedMultiplier;
    }

    void Uncrouching(bool ignoreSpeedChange) {
        RaycastHit2D hit1 = Physics2D.Raycast(_controller.raycastOriginPos.topRight, Vector2.up, 1f + _controller.skinWidth, _controller.levelMask);
        RaycastHit2D hit2 = Physics2D.Raycast(_controller.raycastOriginPos.topLeft, Vector2.up, 1f + _controller.skinWidth, _controller.levelMask);
        if(!hit1 && !hit2) {
            _status.worldState = State.Stand;
            _collider.offset = new Vector2(0, 0.5625f);
            _collider.size = new Vector2(0.50f, 2.25f);
            _hitbox.offset = new Vector2(0, 0.5f);
            _hitbox.size = new Vector2(1, 2);
            _controller.CalculateRaySpacing();
            _camController.ToggleCameraCrouch();
            if(!ignoreSpeedChange)
                _movement.crouchMultiplier = 1f;
        }
    }

    void DropDownPlatform() {
        if(_controller.collision.below && _controller.hitPlatform) {
            _controller.collision.droppingDown = true;
            if(_status.playerState != State.Rolling)
                Uncrouching(false);
        }
    }
}
