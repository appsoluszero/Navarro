using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack1_HandPos : MonoBehaviour
{
    [SerializeField] private Transform targetPosition;
    [SerializeField] private float timeToReachTarget;
    [SerializeField] private AnimationCurve curve;

    bool startCountingDown;
    private float timeCounter = 0f; 
    private Vector3 startPosition;

    void Start() {
        startPosition = transform.position;
    }

    void Update() {
        if(Input.GetKeyDown(KeyCode.P) && startCountingDown == false) {
            startCountingDown = true;
        }

        if(timeCounter >= timeToReachTarget) {
            timeCounter = 0f;
            startCountingDown = false;
        }


        if(startCountingDown) {
            timeCounter += Time.deltaTime;
            float progress = timeCounter / timeToReachTarget;

            transform.position = Vector3.Lerp(startPosition, targetPosition.position, curve.Evaluate(progress));
        }
    }
}
