using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipMovement : MonoBehaviour
{
    public float speed = 1f;
    Quaternion rotationOrigin;
    // Start is called before the first frame update
    void Start()
    {
        rotationOrigin = this.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.rotation = rotationOrigin * Quaternion.Euler(0, 0, Mathf.Sin(speed * Time.time) * 3.0f);
    }
}
