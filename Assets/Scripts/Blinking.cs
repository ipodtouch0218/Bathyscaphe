using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blinking : MonoBehaviour
{
    public float interval;
    new SpriteRenderer renderer;

    void Start() {
        renderer = GetComponent<SpriteRenderer>();
    }
    void Update() {
        renderer.enabled = Time.time%interval < interval/2;
    }
}
