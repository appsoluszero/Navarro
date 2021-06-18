using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageDetection : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D col) {
        if(col.gameObject.tag == "Enemy") {
            if(col.gameObject.name == "Block1") {
                col.transform.parent.GetComponent<Boss_Attack_SlimeBlock>().block1.returnToStart();
                col.transform.parent.GetComponent<Boss_Attack_SlimeBlock>().block1.isRunning = false;
                col.transform.parent.GetComponent<Boss_Attack_SlimeBlock>().block1.timer = 0f;
            }
            else {
                col.transform.parent.GetComponent<Boss_Attack_SlimeBlock>().block2.block.position = col.transform.parent.GetComponent<Boss_Attack_SlimeBlock>().block2.startPos;
                col.transform.parent.GetComponent<Boss_Attack_SlimeBlock>().block2.isRunning = false;
                col.transform.parent.GetComponent<Boss_Attack_SlimeBlock>().block2.timer = 0f;
            }
                
            GetComponent<PlayerStatus>().DecreaseHealth(1);
        }
    }
}
