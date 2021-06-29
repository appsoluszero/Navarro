using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

[RequireComponent(typeof(LineRenderer))]
public class BulletTrace : MonoBehaviour
{
    public AnimationCurve widthOverTime;
    public AnimationCurve alphaOverTime;
    public AnimationCurve lightWidthOverTime;
    public AnimationCurve lightIntensityOverTime;
    public Color color;
    public float lifeTime = 1f;

    [HideInInspector]
    public Vector2 startPos;
    [HideInInspector]
    public Vector2 stopPos;

    private LineRenderer line;
    private Light2D light2D;
    // Start is called before the first frame update
    void Start()
    {
        this.line = GetComponent<LineRenderer>();
        this.light2D = GetComponent<Light2D>();

        this.line.SetPosition(0, startPos);
        this.line.SetPosition(1, stopPos);

        var center = (startPos + stopPos) / 2;
        var span = stopPos.x - startPos.x;

        transform.position = center;
        transform.localScale = new Vector2(span, transform.localScale.y);

        StartCoroutine(Animate());
    }

    IEnumerator Animate()
    {
        var t = 0f;
        var color = this.color;
        var scale = transform.localScale;
        while (t <= 1)
        {
            var width = this.widthOverTime.Evaluate(t);
            var alpha = this.alphaOverTime.Evaluate(t);
            line.widthMultiplier = width;
            color.a = alpha;
            line.startColor = color;
            line.endColor = color;

            var lightWidth = this.lightWidthOverTime.Evaluate(t);
            var lightIntensityOverTime = this.lightIntensityOverTime.Evaluate(t);

            scale.y = lightWidth;
            transform.localScale = scale;
            light2D.intensity = lightIntensityOverTime;

            t += Time.deltaTime / lifeTime;
            yield return new WaitForEndOfFrame();
        }

        Destroy(this.gameObject);
    }
}
