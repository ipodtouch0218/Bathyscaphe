using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTargetX : MonoBehaviour
{
    public Transform target;
    void LateUpdate() {
        transform.position = new Vector3(target.transform.position.x, transform.position.y, transform.position.z);
    }
}
