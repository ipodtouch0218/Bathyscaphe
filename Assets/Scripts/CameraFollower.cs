using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollower : MonoBehaviour
{

    public Vector3 vel = new Vector3(0,0,0);
    public Vector3 offset = new Vector3(0, 0, -10);
    public float smoothing = 0.2f;
    public Transform targetTransform;

    public float shake = 0, shakeMagnitude;
    void LateUpdate() {

        Vector3 target = targetTransform.position + offset;
        if ((shake -= Time.deltaTime) > 0) {
            Vector2 rand = Random.insideUnitCircle * shakeMagnitude;
            target += new Vector3(rand.x, rand.y, 0);
            transform.position = target;
        } else {
            transform.position = Vector3.SmoothDamp(transform.position, target, ref vel, smoothing);
        }
    }
}
