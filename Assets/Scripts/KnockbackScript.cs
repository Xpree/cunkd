using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class KnockbackScript : NetworkBehaviour
{
    [SerializeField] private float KnockbackStrength;
    public bool onOff;

    [ServerCallback]
    private void OnCollisionEnter(Collision collision)
    {
        if(onOff == true)
        {
            Rigidbody rb = collision.collider.GetComponent<Rigidbody>();

            if (rb != null)
            {

                //Forces playerobject into non-grounded state, changes back when it lands when it lands

                if (collision.contactCount == 0)
                    return;

                var settings = GameServer.Instance.Settings;

                Vector3 knockbackforce = collision.relativeVelocity * (KnockbackStrength * (1 + (float)GameServer.startTime.Elapsed * settings.GlobalKnockbackRamp / 100));

                ContactPoint contact = collision.GetContact(0);

                Vector3 horizontalNormal = -contact.normal;
                horizontalNormal.y = 0;
                horizontalNormal = horizontalNormal.normalized;

                Vector3 impulse = (horizontalNormal + Vector3.up * settings.GlobalKnockupMultiplier) * knockbackforce.magnitude * settings.GlobalKnockbackMultiplier;
                var player = rb.GetComponent<PlayerMovement>();
                if (player != null)
                    player.TargetAddforce(impulse, ForceMode.Impulse);
                else
                    rb.AddForce(impulse, ForceMode.Impulse);
            }
        }
        
    }
}
