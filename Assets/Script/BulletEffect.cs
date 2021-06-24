using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletEffect : MonoBehaviour
{
    public ParticleSystem bulletShell;
    public Transform spawnPoint;
    private Transform spriteTransform;

    // Start is called before the first frame update
    void Start()
    {
        this.spriteTransform = transform.Find("Sprite");
        if(this.spriteTransform == null) {
            Debug.LogError("Cannot find child object named `Sprite`");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // code for testing
        if (Input.GetMouseButtonDown(0))
        {
            SpawnBulletShell(1);
        }
    }

    
    public void SpawnBulletShell(int count = 1) {
        var lookDir = this.spawnPoint.right;
        lookDir.x *= this.spriteTransform.localScale.x;
        var rotation = Quaternion.FromToRotation(Vector2.right, lookDir);
        var bulletParticle = Instantiate(this.bulletShell, this.spawnPoint.position, rotation);
        bulletParticle.Emit(count);
    }
}
