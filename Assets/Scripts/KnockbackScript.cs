using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class KnockbackScript : NetworkBehaviour
{
    [SerializeField] private float KnockbackStrength;

    [ServerCallback]
    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = collision.collider.GetComponent<Rigidbody>();

        if (rb != null)
        {

            //Forces playerobject into non-grounded state, changes back when it lands when it lands

            if (collision.contactCount == 0)
                return;

            Vector3 knockbackforce = collision.relativeVelocity * (KnockbackStrength * (1 + (float)GameServer.startTime.Elapsed / 100));

            ContactPoint contact = collision.GetContact(0);

            Vector3 horizontalNormal = -contact.normal;
            horizontalNormal.y = 0;
            horizontalNormal = horizontalNormal.normalized;

            Vector3 impulse = (horizontalNormal + new Vector3(0, 1, 0) ) * knockbackforce.magnitude;
            var player = rb.GetComponent<PlayerMovement>();
            if (player != null)
                player.TargetAddforce(impulse, ForceMode.Impulse);
            else
                rb.AddForce(impulse, ForceMode.Impulse);
        }
    }
}
