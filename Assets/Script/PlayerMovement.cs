using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float jumpHeight = 4;
    [SerializeField] private float timeToMaxHeight = 0.4f;
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float rollingSpeed = 7f;
    [SerializeField] private float timeToFinishRoll = 0.5f;

    [SerializeField] private float accelerationTimeAirborne = 0.2f;
    [SerializeField] private float accelerationTimeGrounded = 0.1f;

    float gravityScale;
    float jumpVelocity;

    Vector3 velocity;
    Vector3 prevVelocity;
    
    float velocityXSmoothing;
    float targetVelocityX;
    float timeRolling;

    Controller2D _controller;

    void Start() {
        InputManager.defaultInitialize();
        _controller = GetComponent<Controller2D>();

        gravityScale = -(2 * jumpHeight) / Mathf.Pow(timeToMaxHeight, 2);
        jumpVelocity = Mathf.Abs(gravityScale * timeToMaxHeight);
    }

    void Update() {
        if(Input.GetKeyDown(InputManager.actionsMap["jumpingUp"]) && _controller.collision.below && !_controller.collision.slidingDownMaxSlope && !_controller.collision.rolling) 
            velocity.y = jumpVelocity;
        if(Input.GetKey(InputManager.actionsMap["dropDown"]) && _controller.collision.below) 
            _controller.collision.droppingDown = true;

        Vector2 input = GetMovementInput();
        targetVelocityX = input.x * moveSpeed;
    }

    void FixedUpdate() {
        prevVelocity = velocity;

        if(Input.GetKeyDown(InputManager.actionsMap["rollingDodge"]) && _controller.collision.below && !_controller.collision.rolling) {
            _controller.collision.rolling = true;
            timeRolling = 0;
        }

        if(_controller.collision.rolling) {
            if(timeRolling > timeToFinishRoll) {
                timeRolling = 0;
                _controller.collision.rolling = false;
            }
            else {
                timeRolling += Time.fixedDeltaTime;
                velocity.x = rollingSpeed * _controller.collision.faceDir;
            }
        }
        else 
            velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (_controller.collision.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
            
        velocity.y += gravityScale * Time.fixedDeltaTime;
        Vector3 deltaPosition = (prevVelocity + velocity) * 0.5f * Time.fixedDeltaTime;
        _controller.Move(deltaPosition);

        if(_controller.collision.above || _controller.collision.below) {
            if(_controller.collision.ascendingSlope && _controller.collision.above)
                velocity.x = 0;
            if(!_controller.collision.slidingDownMaxSlope)
                velocity.y = 0;
        }

        if(_controller.collision.left || _controller.collision.right) {
            velocity.x = 0;
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
}
