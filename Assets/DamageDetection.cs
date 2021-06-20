using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageDetection : MonoBehaviour
{
    [SerializeField] private bool isDamagePlayer;
    void OnTriggerEnter2D(Collider2D col) {
        if(col.gameObject.tag == "SlimeBlock") {
            int num = int.Parse(col.gameObject.name.Substring(5));
            col.transform.parent.GetComponent<Boss_Attack_SlimeBlock>().PlayerHit(num);
        }
        else if(col.gameObject.tag == "FallingObject") {
            col.transform.GetComponentInParent<Boss_Attack_ObjectFall>().ReturnObjectToSpawn(col.gameObject);
        }
        if(isDamagePlayer)
            GetComponent<PlayerStatus>().DecreaseHealth(1);
    }
}
