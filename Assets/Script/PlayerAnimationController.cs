using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator _playerAnimation;
    [SerializeField] private PlayerStatus _status;
    [SerializeField] private Controller2D _controller;
    private Transform _spriteTransform;
    void Start()
    {
        _playerAnimation = GetComponent<Animator>();
        _spriteTransform = GetComponent<Transform>();
    }

    void Update()
    {
        if(_status.playerState == State.Rolling)
            _playerAnimation.Play("Rolling");
        else if(_status.playerState == State.Attack) {
            if(_status.worldState == State.Crouching)
                _playerAnimation.Play("Attack_Crouch"); 
            else {
                if(_status.worldState == State.Floating_Crouching)
                    _playerAnimation.Play("Attack_Crouch");
                else
                    _playerAnimation.Play("Attack_Stand"); 
            }      
        }
        else if(_status.playerState == State.Move) {
            if(_status.worldState == State.Crouching)
                _playerAnimation.Play("Move_Crouch"); 
            else {
                if(_status.worldState == State.Floating_Crouching)
                    _playerAnimation.Play("Move_Crouch");
                else
                    _playerAnimation.Play("Move_Stand"); 
            }  
        }
        else {
            if(_status.worldState == State.Crouching)
                _playerAnimation.Play("Idle_Crouch"); 
            else {
                if(_status.worldState == State.Floating_Crouching)
                    _playerAnimation.Play("Idle_Crouch");
                else
                    _playerAnimation.Play("Idle_Stand");
            }  
        }
    }
}
