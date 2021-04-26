using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthColorChanger : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    public Color starting;
    public Color ending;
    public float maxdepth = 120f;

    void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    void Update()
    {
        spriteRenderer.color = LerpColor(ending, starting, Mathf.Clamp(1-(-transform.position.y/maxdepth), 0, 1));
    }

    Color LerpColor(Color color1, Color color2, float t) {
        float r = Mathf.Lerp(color1.r, color2.r, t);
        float g = Mathf.Lerp(color1.g, color2.g, t);
        float b = Mathf.Lerp(color1.b, color2.b, t);
        float a = Mathf.Lerp(color1.a, color2.a, t);
        return new Color(r,g,b,a);
    }
}
