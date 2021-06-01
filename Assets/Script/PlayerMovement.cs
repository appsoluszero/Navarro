using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class PlayerMovement : MonoBehaviour
{
    
    private PlayerStatus _status;
    private PlayerAction _action;

    [Header("Moving Parameters")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float accelTimeGrounded = 0.05f;


    [Header("Airborne Parameters")]
    [SerializeField] private float accelTimeAirborne = 0.15f;

    float gravityScale;
    public float jumpVelocity;

    [HideInInspector] public Vector3 velocity;
    Vector3 prevVelocity;
    
    float velocityXSmoothing;
    float targetVelocityX;
    Vector2 input;
    [HideInInspector] public float crouchMultiplier;

    Controller2D _controller;
    

    void Start() {
        InputManager.defaultInitialize();
        _controller = GetComponent<Controller2D>();
        _status = GetComponent<PlayerStatus>();
        _action = GetComponent<PlayerAction>();

        gravityScale = -(2 * _action.jumpHeight) / Mathf.Pow(_action.timeToMaxHeight, 2);
        jumpVelocity = Mathf.Abs(gravityScale * _action.timeToMaxHeight);
        crouchMultiplier = 1f;
    }

    void Update() {

        _action.CheckForInputAction();

        if(!_controller.collision.below && !_controller.collision.above && !_controller.collision.onPlatform) {
            if(_status.worldState == State.Stand) 
                _status.worldState = State.Floating_Stand;
            else if(_status.worldState == State.Crouch)
                _status.worldState = State.Floating_Crouch;
        }
        else if(_controller.collision.below) {
            if(_status.worldState == State.Floating_Stand) 
                _status.worldState = State.Stand;
            else if(_status.worldState == State.Floating_Crouch) 
                _status.worldState = State.Crouch;
        }
        
        input = GetMovementInput();

        targetVelocityX = input.x * moveSpeed * crouchMultiplier;
    }

    void FixedUpdate() {
        prevVelocity = velocity;
        
        if(_status.playerState != State.Rolling && _status.playerState != State.Attack) {
            _status.playerState = State.Idle;
            velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (_controller.collision.below) ? accelTimeGrounded : accelTimeAirborne);
        }

        if(_status.playerState == State.Attack && _controller.collision.below)
            velocity.x = 0;
            
        velocity.y += gravityScale * Time.fixedDeltaTime;
        Vector3 deltaPosition = (prevVelocity + velocity) * 0.5f * Time.fixedDeltaTime;

        _controller.Move(deltaPosition);

        if(_controller.collision.above || _controller.collision.below) {
            if(_controller.collision.ascendingSlope && _controller.collision.above)
                velocity.x = 0;
            velocity.y = 0f;    
        }

        if(_controller.collision.left || _controller.collision.right) {
            velocity.x = 0;
        }

        if(Mathf.Abs(velocity.x) >= 1E-2 && _status.playerState == State.Idle) 
            _status.playerState = State.Move;
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
}
