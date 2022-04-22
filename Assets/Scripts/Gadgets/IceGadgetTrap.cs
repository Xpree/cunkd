using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class IceGadgetTrap : NetworkBehaviour
{
    [SerializeField] GameSettings _settings;
    [SyncVar] NetworkTimer _endTime;
    public float friction => _settings.IceGadget.Friction;


    public override void OnStartServer()
    {
        if (_settings == null)
        {
            Debug.LogError("Missing GameSettings reference on " + name);
        }

        _endTime = NetworkTimer.FromNow(_settings.IceGadget.Duration);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            other.gameObject.GetComponent<PlayerMovement>().maxFrictionScaling = friction;
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
        if (_endTime.HasTicked)
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
    }
}
