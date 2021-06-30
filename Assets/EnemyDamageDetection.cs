using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamageDetection : MonoBehaviour
{
    [SerializeField] private float enemyHealth = 100f;

    public void receiveDamage(float damage, PlayerStatus stat, PlayerAttack atk) {
        GetComponent<RunnerAI>().TakeDamage();
        enemyHealth = Mathf.Clamp(enemyHealth - damage, 0f, 100f);
        if(enemyHealth == 0) {
            int chance = Random.Range(0, 101);
            if(chance <= 45 && stat && atk) {
                stat.bulletCount++;
                atk._statusUI.UpdateBullet();
            }
            Destroy(this.gameObject);
        }
    }
}
