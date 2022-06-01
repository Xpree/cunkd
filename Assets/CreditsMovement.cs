using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsMovement : MonoBehaviour
{
    // Start is called before the first frame update

    Vector3 origin;
    float timeStart;

    public float distance = 2000.0f;
    public float speed = 0.04f;
    void Start()
    {
        origin = this.transform.localPosition;
        timeStart = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        var x = ((Time.time - timeStart) * speed) % 1.0f;
        this.transform.localPosition = origin + Vector3.up * x * distance;
    }
}
