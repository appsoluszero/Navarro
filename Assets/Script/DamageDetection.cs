using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageDetection : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ranger_Bullet"))
        {
            print("Bullet hit");
            GetComponent<PlayerStatus>().DecreaseHealth(1);
            Destroy(other.gameObject);
        }
    }
}
