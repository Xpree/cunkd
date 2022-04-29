using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderTimer : MonoBehaviour
{
    [SerializeField] float colliderTimer;
    float elapsedTime =0;

    bool isSet = false;
    void setColliderEnabled()
    {
        Collider col = GetComponent<Collider>();
        print("collider belongs to: " + col.gameObject.name);
        col.isTrigger = true;

        isSet = true;
    }

    private void FixedUpdate()
    {
        elapsedTime += Time.fixedDeltaTime;
        if (!isSet && colliderTimer < elapsedTime)
        {
            setColliderEnabled();
        }
    }
}
