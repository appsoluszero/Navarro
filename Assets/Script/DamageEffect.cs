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

    public void DoBlood(Vector3 attackerPosition)
    {
        var relativePosition = transform.position - attackerPosition;


        var blood_instance = Instantiate(
            this.damageEffectData.blood_particle,
            transform.position,
            Quaternion.FromToRotation(Vector3.right, relativePosition),
            transform
        );
        blood_instance.transform.localPosition = damageEffectData.relativePosition;
        
        // blood_instance.transform.localPosition = new Vector3(-0.2f, 0.64f, 0.0f);
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
