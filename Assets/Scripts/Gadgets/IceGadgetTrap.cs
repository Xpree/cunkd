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

    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Player")
        {
            other.gameObject.GetComponent<PlayerMovement>().maxFrictionScaling = friction;
            other.gameObject.GetComponent<PlayerMovement>().maxSpeedScaling = 0.01f;
        }
    }

    void FixedUpdate()
    {
        if (_endTime.HasTicked)
        {
            if (NetworkServer.active)
            {
                NetworkServer.Destroy(this.gameObject);
            }
            return;
        }
    }
}
