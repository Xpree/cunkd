using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class IceGadgetTrap : NetworkBehaviour
{
    public PhysicMaterial friction;
    public PhysicMaterial noFriction;
    public float duration = 30f;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            other.gameObject.GetComponent<PlayerMovement>().maxFrictionScaling = -0.5f;
            other.gameObject.GetComponent<PlayerMovement>().maxSpeedScaling = 0f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player")
        {
            other.gameObject.GetComponent<PlayerMovement>().maxSpeedScaling = 1f;
            other.gameObject.GetComponent<PlayerMovement>().maxFrictionScaling = 1f;
        }
    }

    void FixedUpdate()
    {

        if (duration <= 0)
        {
            if (NetworkServer.active)
            {
                var collisions = Physics.OverlapSphere(this.transform.position, this.GetComponent<CapsuleCollider>().radius);
                foreach (var collision in collisions)
                {
                    if (collision != null && collision.tag == "Player")
                    {
                        collision.gameObject.GetComponent<PlayerMovement>().maxSpeedScaling = 1f;
                        collision.gameObject.GetComponent<PlayerMovement>().maxFrictionScaling = 1f;
                    }
                }
                NetworkServer.Destroy(this.gameObject);
            }
            return;
        }
        duration = duration - Time.fixedDeltaTime;
    }
}
