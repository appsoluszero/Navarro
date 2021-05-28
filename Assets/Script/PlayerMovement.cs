using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class PlayerMovement : MonoBehaviour
{
    private PlayerCameraController _camController;
    private PlayerStatus _status;

    [Header("Jumping Parameters")]
    [SerializeField] private float jumpHeight = 4;
    [SerializeField] private float timeToMaxHeight = 0.4f;

    [Header("Moving Parameters")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float accelTimeGrounded = 0.05f;

    [Header("Rolling Parameter")]
    [SerializeField] private float rollingSpeed = 7f;
    [SerializeField] private float timeToFinishRoll = 0.5f;

    [Header("Crouching Parameters")]
    [SerializeField] private float crouchSpeedMultiplier = 0.5f; 

    [Header("Airborne Parameters")]
    [SerializeField] private float accelTimeAirborne = 0.15f;
    [Header("Platform Mask")]
    [SerializeField] private LayerMask platformMask;

    float gravityScale;
    float jumpVelocity;

    [SerializeField] Vector3 velocity;
    Vector3 prevVelocity;
    
    float velocityXSmoothing;
    float targetVelocityX;
    Vector2 input;
    float crouchMultiplier;

    Controller2D _controller;
    BoxCollider2D _collider;
    BoxCollider2D _hitbox;

    void Start() {
        InputManager.defaultInitialize();
        _controller = GetComponent<Controller2D>();
        _collider = GetComponent<BoxCollider2D>();
        _camController = transform.GetChild(0).GetComponent<PlayerCameraController>();
        _hitbox = transform.GetChild(1).GetComponent<BoxCollider2D>();
        _status = GetComponent<PlayerStatus>();

        gravityScale = -(2 * jumpHeight) / Mathf.Pow(timeToMaxHeight, 2);
        jumpVelocity = Mathf.Abs(gravityScale * timeToMaxHeight);
        crouchMultiplier = 1f;
    }

    void Update() {
        if(_controller.collision.below && !_controller.collision.rolling && _status.playerState != State.Attack) {
            if(Input.GetKeyDown(InputManager.actionsMap["jumpingUp"])) {
                velocity.y = jumpVelocity;
                crouchingAction(true, false);
            }
            if(!_controller.collision.slidingDownMaxSlope) {
                if(Input.GetKeyDown(InputManager.actionsMap["rollingDodge"])) {
                    _controller.collision.rolling = true;
                    crouchingAction(false, true);
                    crouchMultiplier = 1f;
                    StartCoroutine(rollingSequence());
                }
                else if(Input.GetKeyDown(InputManager.actionsMap["crouchDown"])) {
                    crouchingAction(false, false);
                }
            }
        }

        if(Input.GetKey(InputManager.actionsMap["dropDown"]) && _controller.collision.below) {
            _controller.collision.droppingDown = true;
            crouchingAction(true, false);
            crouchMultiplier = 1f;
        }

        _status.worldState = State.Standing;
        if(!_controller.collision.below) {
            if(_controller.collision.crouching)
                _status.worldState = State.Floating_Crouching;
            else
                _status.worldState = State.Floating_Standing;
        }
        else if(_controller.collision.crouching)
            _status.worldState = State.Crouching;

        input = GetMovementInput();
        if(_controller.collision.slidingDownMaxSlope) {
            _status.playerState = State.Sliding;
        }
        else if(_controller.collision.rolling) {
            _status.playerState = State.Rolling;
        }
        else {
            if(_status.playerState != State.Attack) {
                if(input != Vector2.zero) 
                    _status.playerState = State.Move;
                else
                    _status.playerState = State.Idle;
            }
        }

        targetVelocityX = input.x * moveSpeed * crouchMultiplier;
    }

    void FixedUpdate() {
        prevVelocity = velocity;

        if(_status.playerState == State.Attack && !_controller.collision.below) 
                velocity.x = 0;
        
        if(!_controller.collision.rolling && !_controller.collision.slidingDownMaxSlope) 
            velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (_controller.collision.below) ? accelTimeGrounded : accelTimeAirborne);
            
        velocity.y += gravityScale * Time.fixedDeltaTime;
        Vector3 deltaPosition = (prevVelocity + velocity) * 0.5f * Time.fixedDeltaTime;

        if(_status.playerState == State.Attack && _controller.collision.below)
                deltaPosition.x = 0;

        _controller.Move(deltaPosition);

        if(_controller.collision.above || _controller.collision.below) {
            if(_controller.collision.ascendingSlope && _controller.collision.above)
                velocity.x = 0;
            if(!_controller.collision.slidingDownMaxSlope)
                velocity.y = 0f;    
        }

        if(_controller.collision.left || _controller.collision.right) {
            velocity.x = 0;
        }
    }

    void crouchingAction(bool forceUnCrouch, bool forceCrouch) {
        if(!_controller.collision.crouching && !forceUnCrouch || forceCrouch) {
            _controller.collision.crouching = true;
            _collider.offset = new Vector2(0, 0);
            _collider.size = new Vector2(1, 1);
            _hitbox.offset = new Vector2(0, 0);
            _hitbox.size = new Vector2(1, 1);
            _controller.CalculateRaySpacing();
            _camController.ToggleCameraCrouch();
            crouchMultiplier = crouchSpeedMultiplier;
        }
        else {
            RaycastHit2D hit1 = Physics2D.Raycast(_controller.raycastOriginPos.topRight, Vector2.up, 1f + _controller.skinWidth, _controller.collisionMask);
            RaycastHit2D hit2 = Physics2D.Raycast(_controller.raycastOriginPos.topLeft, Vector2.up, 1f + _controller.skinWidth, _controller.collisionMask);
            if(!hit1 && !hit2) {
                _controller.collision.crouching = false;
                _collider.offset = new Vector2(0, 0.5f);
                _collider.size = new Vector2(1, 2);
                _hitbox.offset = new Vector2(0, 0.5f);
                _hitbox.size = new Vector2(1, 2);
                _controller.CalculateRaySpacing();
                _camController.ToggleCameraCrouch();
                crouchMultiplier = 1f;
            }
        }
    }

    Vector2 GetMovementInput() {
        float vertical = 0;
        if(Input.GetKey(InputManager.actionsMap["walkRight"])) {
            vertical = 1;
        }
        else if(Input.GetKey(InputManager.actionsMap["walkLeft"])) {
            vertical = -1;
        }
        return new Vector2(vertical, 0);
    }

    IEnumerator rollingSequence() {
        velocity.x = rollingSpeed * _controller.collision.faceDir;
        StartCoroutine(_status.decreaseStamina());
        yield return new WaitForSeconds(timeToFinishRoll);
        crouchingAction(true, false);
        _controller.collision.rolling = false;
    }
}