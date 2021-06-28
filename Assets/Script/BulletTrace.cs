using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTrace : MonoBehaviour
{
    public AnimationCurve widthOverTime;
    public AnimationCurve alphaOverTime;
    public Color color;
    private LineRenderer line;
    // Start is called before the first frame update
    void Start()
    {
        this.line = GetComponent<LineRenderer>();
        StartCoroutine(Animate());
    }

    IEnumerator Animate() {
        var t = 0f;
        var color = this.color;
        while (t <= 1) {
            var width = this.widthOverTime.Evaluate(t);
            var alpha = this.alphaOverTime.Evaluate(t);
            line.widthMultiplier = width;
            color.a = alpha;
            line.startColor = color;
            line.endColor = color;
            t += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        
        Destroy(this.gameObject);
    }
}
