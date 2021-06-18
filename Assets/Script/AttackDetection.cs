using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackDetection : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private LayerMask enemyLayerMask;
    [SerializeField] private LayerMask collisionMask;
    //Melee detection
    void OnTriggerEnter2D(Collider2D col) {
        if(col.gameObject.tag == "Enemy" && ((enemyLayerMask & 1 << col.gameObject.layer) != 0)) {
            Vector2 dir = new Vector2(col.transform.position.x - playerTransform.position.x, 0f).normalized;
            col.transform.GetComponent<Rigidbody2D>().AddForce(Vector2.right * dir * 1.25f, ForceMode2D.Impulse);
        }
    }

    //Ranged detection
    public void HitscanCheck(float range, int penetrate) {
        float actualRange;
        //check for max distance
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector3.right * transform.parent.parent.GetComponent<Controller2D>().collision.faceDir, range, collisionMask);
        if(hit) 
            actualRange = hit.distance;
        else
            actualRange = range;
        Debug.DrawRay(transform.position, Vector3.right * transform.parent.parent.GetComponent<Controller2D>().collision.faceDir * actualRange, Color.green, 20f);
        //list of enemy in hitting range
        RaycastHit2D[] enemyHit = Physics2D.RaycastAll(transform.position, Vector3.right * transform.parent.parent.GetComponent<Controller2D>().collision.faceDir, actualRange, enemyLayerMask);
        if(enemyHit.Length > penetrate)
            Array.Resize(ref enemyHit, penetrate);
        foreach(RaycastHit2D e in enemyHit) {
            print(e.transform.gameObject.name);
            Rigidbody2D rb_this = e.transform.GetComponent<Rigidbody2D>();
            Vector2 dir = new Vector2(e.transform.position.x - playerTransform.position.x, 0f).normalized;
            rb_this.AddForce(Vector2.right * dir * 2.25f, ForceMode2D.Impulse);
        }
    }
}
