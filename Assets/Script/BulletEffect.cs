using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletEffect : MonoBehaviour
{
    public ParticleSystem bulletShell;
    public Transform spawnPoint;
    public BulletTrace traceLine;
    [SerializeField] private bool isDebug;
    private Controller2D controller2D;

    private void Start() {
        this.controller2D = transform.parent.GetComponent<Controller2D>();
    }

    void FixedUpdate() {
        if(isDebug) {
            if(Input.GetMouseButtonDown(0)) {
                SpawnBulletShell(1);
                SpawnBulletTrace(transform.position, transform.position + new Vector3(1, 1, 0));
            }
        }
    }
    
    public void SpawnBulletShell(int count = 1) {
        print("test");
        var dir = this.controller2D.collision.faceDir;

        var lookDir = this.spawnPoint.right;
        lookDir.x *= dir;
        var rotation = Quaternion.FromToRotation(Vector2.right, lookDir);
        var pos = this.spawnPoint.position;
        if(dir > 0) {
             pos.x = 2 * transform.parent.position.x - pos.x;
        }

        var bulletParticle = Instantiate(this.bulletShell, pos, rotation);
        bulletParticle.Emit(count);
    }

    public void SpawnBulletTrace(Vector2 start, Vector2 stop) {
        var line = Instantiate(this.traceLine);
        line.startPos = start;
        line.stopPos = stop;
    }
}
