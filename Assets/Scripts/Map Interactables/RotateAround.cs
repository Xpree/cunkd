using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAround : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] Vector3 target;

    private void FixedUpdate()
    {
        transform.RotateAround(target, Vector3.up, speed*Time.fixedDeltaTime);
        transform.localRotation = Quaternion.identity;
    }
}
