using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Boss_Attack_SlimeBlock : MonoBehaviour
{
    [SerializeField] private float timeToReachTarget = 0.75f;
    [Range(0f, 2f)][SerializeField] private float timeInBetween = 1f;
    public struct slimeBlock {
        public Transform block;
        public Vector3 startPos, targetPos;
        public bool isRunning;
        public float timer;

        public slimeBlock(Transform bRef, Vector3 targetPos) {
            block = bRef;
            startPos = bRef.position;
            this.targetPos = targetPos;
            isRunning = false;
            timer = 0f;
        }

        public void returnToStart() {
            block.position = startPos;
        }
    }

    public slimeBlock block1, block2;
    bool isRunning;
    float timerBetween = 0;

    void Start() {
        block1 = new slimeBlock(transform.GetChild(0), transform.GetChild(2).position);
        block2 = new slimeBlock(transform.GetChild(1), transform.GetChild(3).position);
    }

    void Update() {
        if(Input.GetKeyDown(KeyCode.Space) && !block1.isRunning && !block2.isRunning) {
            block1.isRunning = true;
            isRunning = true;
        }

        if(timerBetween >= timeInBetween && isRunning) {
            block2.isRunning = true;
            isRunning = false;
            timerBetween = 0f;
        }

        if(isRunning) 
            timerBetween += Time.deltaTime;

        if(block1.timer >= timeToReachTarget) {
            block1.isRunning = false;
            block1.timer = 0f;
            block1.returnToStart();
        }

        if(block2.timer >= timeToReachTarget) {
            block2.isRunning = false;
            block2.timer = 0f;
            block2.returnToStart();
        }

        if(block1.isRunning) {
            block1.timer += Time.deltaTime;
            block1.timer = Mathf.Clamp(block1.timer, 0f, timeToReachTarget);
            AttackSequence(block1);
        }

        if(block2.isRunning) {
            block2.timer += Time.deltaTime;
            block2.timer = Mathf.Clamp(block2.timer, 0f, timeToReachTarget);
            AttackSequence(block2);
        }
    }

    void AttackSequence(slimeBlock block) {
        float progress = block.timer / timeToReachTarget;
        block.block.position = Vector3.Lerp(block.startPos, block.targetPos, progress);
    }
}
