using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private PlayerStatus _status;

    void Start() {
        _status = GetComponent<PlayerStatus>();
    }

    void Update() {
        if(Input.GetKeyDown(KeyCode.Mouse0) && _status.playerState != State.Attack) {
            StartCoroutine(attackSequence());
        } 
    }

    IEnumerator attackSequence() {
        _status.playerState = State.Attack;
        yield return new WaitForSeconds(1.0f);
        _status.playerState = State.Idle;
    }
}
