using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private PlayerStatus _status;
    private Controller2D _controller;

    void Start() {
        _status = GetComponent<PlayerStatus>();
        _controller = GetComponent<Controller2D>();
    }

    void Update() {
        if(Input.GetKeyDown(KeyCode.Mouse0) && _status.playerState != State.Attack && !_controller.collision.rolling && !_controller.collision.slidingDownMaxSlope) {
            StartCoroutine(attackSequence());
        } 
    }

    IEnumerator attackSequence() {
        _status.playerState = State.Attack;
        print("ATTACK");
        yield return new WaitForSeconds(0.45f);
        _status.playerState = State.Idle;
    }
}
