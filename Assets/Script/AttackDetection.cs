using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackDetection : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private LayerMask enemyLayerMask;
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private Transform tracerSpawnPoint;

    private Controller2D _controller;
    private PlayerCameraController _camController;
    private PlayerStatus _status;
    private PlayerAttack _attack;
    private BulletEffect _bulletEffect;

    void Start() {
        _controller = transform.parent.GetComponentInParent<Controller2D>();
        _camController = transform.parent.parent.GetComponentInChildren<PlayerCameraController>();
        _bulletEffect = transform.parent.GetComponent<BulletEffect>();
        _attack = transform.parent.GetComponent<PlayerAttack>();
        _status = transform.parent.GetComponentInParent<PlayerStatus>();
    }
    
    //Melee detection
    void OnTriggerEnter2D(Collider2D col) {
        if(col.gameObject.tag == "Enemy" && ((enemyLayerMask & 1 << col.gameObject.layer) != 0)) {
            _attack.isHitThisAttack = true;
            col.transform.GetComponent<DamageEffect>().DoEffect(transform.parent.parent.position);
            col.transform.GetComponent<EnemyDamageDetection>().receiveDamage(_attack.meleeDmg, _status, _attack);
            Vector2 dir = new Vector2(col.transform.position.x - playerTransform.position.x, 0f).normalized;
            col.transform.GetComponent<Rigidbody2D>().AddForce(Vector2.right * dir * 1.25f, ForceMode2D.Impulse);
            _camController.AttackCameraShake(true);
        }
    }

    //Ranged detection
    public float HitscanCheck(float range, int penetrate) {
        float actualRange;
        //check for max distance
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector3.right * _controller.collision.faceDir, range, collisionMask);
        if(hit) 
            actualRange = hit.distance;
        else
            actualRange = range;
        Debug.DrawRay(transform.position, Vector3.right * _controller.collision.faceDir * actualRange, Color.green, 20f);
        _bulletEffect.SpawnBulletTrace(tracerSpawnPoint.position, tracerSpawnPoint.position + Vector3.right * _controller.collision.faceDir * actualRange);
        //list of enemy in hitting range
        RaycastHit2D[] enemyHit = Physics2D.RaycastAll(transform.position, Vector3.right * _controller.collision.faceDir, actualRange, enemyLayerMask);
        if(enemyHit.Length > penetrate)
            Array.Resize(ref enemyHit, penetrate);
        foreach(RaycastHit2D e in enemyHit) {
            Rigidbody2D rb_this = e.transform.GetComponent<Rigidbody2D>();
            e.transform.GetComponent<DamageEffect>().DoEffect(e.transform.position + Vector3.forward);
            e.transform.GetComponent<EnemyDamageDetection>().receiveDamage(_attack.rangedDmg, null, null);
            Vector2 dir = new Vector2(e.transform.position.x - playerTransform.position.x, 0f).normalized;
            rb_this.AddForce(Vector2.right * dir * 2.25f, ForceMode2D.Impulse);
        }
        return actualRange;
    }
}
