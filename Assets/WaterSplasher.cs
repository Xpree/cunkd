using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSplasher : MonoBehaviour
{
    [SerializeField] ParticleSystem splash;
    [SerializeField] float triggerHeight=0;

    private void OnTriggerEnter(Collider other)
    {
        ParticleSystem pe = Instantiate(splash, other.transform.position + new Vector3(0, triggerHeight, 0), splash.transform.rotation);
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb)
        {

            pe.transform.localScale += Vector3.one * Mathf.Log(rb.mass) / 5;
        }
    }
}
