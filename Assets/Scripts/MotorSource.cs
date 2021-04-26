using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotorSource : MonoBehaviour
{
    public float minPitch = 1f, pitchSlope = 0.1f;
    public Rigidbody2D targetBody;
    AudioSource source;

    void Start() {
        source = GetComponent<AudioSource>();
    }
    void FixedUpdate() {
        source.pitch = pitchSlope * Mathf.Abs(targetBody.velocity.magnitude) + minPitch;
    }
}
