using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageEffect : MonoBehaviour
{
    public DamageEffectData damageEffectData;

    private Material material;
    private Coroutine flashHandle;

    // Start is called before the first frame update
    void Start()
    {
        material = GetComponentInChildren<SpriteRenderer>().material;
    }

    // Update is called once per frame
    /*void Update()
    {
        // just for testing
        if (Input.GetMouseButtonDown(0))
        {
            var playerPos = GameObject.FindGameObjectWithTag("Player").transform.position;
            DoEffect(playerPos);
        }
        if (Input.GetMouseButtonDown(1))
        {
            DoEffect(transform.position + Vector3.forward);
        }
    }*/

    public void DoEffect(Vector3 attackerPosition)
    {

        if (damageEffectData.enable)
        {
            DoFlash();
            DoBlood(attackerPosition);
        }
    }

    public void DoFlash()
    {
        if (flashHandle != null)
        {
            StopCoroutine(flashHandle);
        }
        this.flashHandle = StartCoroutine(FlashRoutine());
    }

    public ParticleSystem DoBlood(Vector3 attackerPosition)
    {
        var relativePosition = transform.position - attackerPosition;


        var blood_instance = Instantiate(
            this.damageEffectData.blood_particle,
            transform.position,
            Quaternion.FromToRotation(Vector3.right, relativePosition),
            transform
        );
        blood_instance.transform.localPosition = damageEffectData.relativePosition;
        blood_instance.transform.SetParent(null);
        return blood_instance;
        // blood_instance.transform.localPosition = new Vector3(-0.2f, 0.64f, 0.0f);
    }

    public void DoBloodExplosion(Rect area) {
        var blood_count = this.damageEffectData.bloodDensity * area.width * area.height;
        for (int i = 0; i < blood_count; i++)
        {
            // rejection sampling with maxmimum rejection count of 10
            for(int b=0; b<10 ; b++) {
                var x = UnityEngine.Random.Range(area.xMin, area.xMax);   
                var y = UnityEngine.Random.Range(area.yMin, area.yMax);
                var point = new Vector2(x, y);
                if(!Physics2D.OverlapPoint(point, this.damageEffectData.bloodSpawnMask)) {
                    Instantiate(this.damageEffectData.bloodBombParticle, point, Quaternion.identity);
                    break;
                }
            }
        }
    }

    public void DoBloodExplosion(Bounds area) {
        var min = area.min;
        var size = area.size;
        DoBloodExplosion(new Rect(min.x, min.y, size.x, size.y));
    }

    IEnumerator FlashRoutine()
    {
        float resoultion = 1.0f / (float)(this.damageEffectData.frameCount);
        float t = 0;

        while (t <= 1.0)
        {
            float flashVal = this.damageEffectData.flashCurve.Evaluate(t);
            material.SetFloat("_Flash", flashVal);
            t += resoultion;
            yield return new WaitForFixedUpdate();
        }
        material.SetFloat("_Flash", 0f);
    }
}
