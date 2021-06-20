using System.Collections;
using UnityEngine;

public class Boss_Attack_SlimeBlock : MonoBehaviour, IBossAttack
{
    [SerializeField] private float timeToReachTarget = 0.75f;
    [SerializeField] private float timeInBetween = 1f;
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
    public slimeBlock[] blockNow;
    public int queue = 1;

    void Start() {
        blockNow = new slimeBlock[transform.childCount / 2];
        for(int i = 0 ; i < blockNow.Length ; ++i) {
            blockNow[i] = new slimeBlock(transform.GetChild(i), transform.GetChild(i+2).position);
        }
    }

    public void AttackLogicRunner() {
        bool isAllSleep = true;
        foreach(slimeBlock k in blockNow) {
            if(k.isRunning)
                isAllSleep = false;
        }
        if(Input.GetKeyDown(KeyCode.Space) && isAllSleep) {
            blockNow[0].isRunning = true;
            StartCoroutine(countDownEachBlock());
        }
        for(int i = 0 ; i < blockNow.Length ; ++i) {
            if(blockNow[i].timer >= timeToReachTarget) {
                blockNow[i].isRunning = false;
                blockNow[i].timer = 0f;
                blockNow[i].returnToStart();
            }
            if(blockNow[i].isRunning) {
                blockNow[i].timer += Time.deltaTime;
                blockNow[i].timer = Mathf.Clamp(blockNow[i].timer, 0f, timeToReachTarget);
                AttackSequence(blockNow[i]);
            }
        } 
    }

    IEnumerator countDownEachBlock() {
        yield return new WaitForSeconds(timeInBetween);
        if(blockNow.Length > queue) {
            blockNow[queue].isRunning = true;
            queue++;
            StartCoroutine(countDownEachBlock());
        }
        else
            queue = 1;
    }

    void AttackSequence(slimeBlock block) {
        float progress = block.timer / timeToReachTarget;
        block.block.position = Vector3.Lerp(block.startPos, block.targetPos, progress);
    }

    public void PlayerHit(int num) {
        blockNow[num-1].returnToStart();
        blockNow[num-1].isRunning = false;
        blockNow[num-1].timer = 0f;
    }
}
