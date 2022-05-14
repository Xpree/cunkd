using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolcanoErupter : MonoBehaviour
{
    [SerializeField] float explosionForce;
    [SerializeField] float explosionRadius;
    [SerializeField] Transform explosionPosition;

    //private void OnTriggerEnter(Collider other)
    //{
    //    Rigidbody rigidbody = other.GetComponent<Rigidbody>();
    //    if (rigidbody)
    //    {
    //        print("rigid enter");
    //        rigidbody.AddForce(Vector3.up * (explosionForce / ((other.transform.position - explosionPosition.position).magnitude) + 10), ForceMode.Impulse);
    //        rigidbody.AddExplosionForce(explosionForce, explosionPosition.position, explosionRadius);
    //    }
    //}

    private void OnTriggerStay(Collider other)
    {
        Rigidbody rigidbody = other.GetComponent<Rigidbody>();
        if (rigidbody)
        {
            print("rigid enter");
            rigidbody.AddForce((Vector3.up * explosionForce / Mathf.Abs(other.transform.position.y - explosionPosition.position.y) + new Vector3(Random.Range(-1f,1f),0,Random.Range(-1f,1f))), ForceMode.Impulse);
        }
    }
}
