using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class KnockbackScript : NetworkBehaviour
{
    [SerializeField] private float KnockbackStrength;

    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = collision.collider.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Vector3 direction = collision.transform.position - transform.position;

            rb.AddForce(direction * KnockbackStrength, ForceMode.Impulse);
        }
    }
}
