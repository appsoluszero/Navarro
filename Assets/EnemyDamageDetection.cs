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
            int chance_bullet = Random.Range(0, 101);
            if(chance_bullet <= 45 && stat && atk) {
                stat.bulletCount++;
                atk._statusUI.UpdateBullet();
            }
            int chance_heal = Random.Range(0, 101);
            if(chance_heal <= 50 && stat) {
                stat.IncreaseHealth(1);
            }
            PlayerPrefs.SetInt("KillCount", PlayerPrefs.GetInt("KillCount")+1);

            var bound = transform.Find("Sprite").GetComponent<SpriteRenderer>().bounds;
            GetComponent<DamageEffect>().DoBloodExplosion(bound);
            
            Destroy(this.gameObject);
        }
    }
}
