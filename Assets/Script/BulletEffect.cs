using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletEffect : MonoBehaviour
{
    public ParticleSystem bulletShell;
    public Transform spawnPoint;
    [SerializeField] private bool isDebug;

    void Update() {
        if(isDebug) {
            if(Input.GetMouseButtonDown(0)) {
                SpawnBulletShell(1);
            }
        }
    }
 
    public void SpawnBulletShell(int count = 1) {
        print("test");
        var lookDir = this.spawnPoint.right;
        lookDir.x *= transform.parent.localScale.x;
        var rotation = Quaternion.FromToRotation(Vector2.right, lookDir);
        var bulletParticle = Instantiate(this.bulletShell, this.spawnPoint.position, rotation);
        bulletParticle.Emit(count);
    }
}
