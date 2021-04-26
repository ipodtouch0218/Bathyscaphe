using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class LightDimmer : MonoBehaviour {

    public float m, b, min;
    Light2D l2d;
    void Start() {
        l2d = GetComponent<Light2D>();
    }

    void Update() {
        l2d.intensity = Mathf.Max(min, m * transform.position.y + b);
    }
    
}
