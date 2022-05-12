using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class IceGadgetTrap : NetworkBehaviour
{
    [SerializeField] GameSettings _settings;
    [SyncVar] NetworkTimer _endTime;
    public float friction => _settings.IceGadget.Friction;

    [SerializeField] SpreadMat iceMachine;

    public override void OnStartServer()
    {
        if (_settings == null)
        {
            Debug.LogError("Missing GameSettings reference on " + name);
        }

        _endTime = NetworkTimer.FromNow(_settings.IceGadget.Duration);
    }

    public void makePlayerSlip(GameObject player)
    {
        player.GetComponent<PlayerMovement>().maxFrictionScaling = friction;
        player.GetComponent<PlayerMovement>().maxSpeedScaling = 0f;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            other.gameObject.GetComponent<PlayerMovement>().maxFrictionScaling = friction;
            other.gameObject.GetComponent<PlayerMovement>().maxSpeedScaling = 0.01f;
        }
    }

    [Client]
    void destroyIce()
    {
        iceMachine.unFreezeObjects();
        foreach (var item in iceMachine.iceMat)
        {
            Destroy(item);
        }
    }

    //bool triggered = false;
    //private void OnTriggerEnter(Collider other)
    //{
    //    if (!triggered)
    //    {
    //        iceMachine.Trigger();
    //        triggered = true;
    //    }
    //}

    bool triggered = false;
    private void OnCollisionEnter(Collision collision)
    {
        if (!triggered && collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {

            GetComponent<Rigidbody>().isKinematic = true;
            iceMachine.Trigger();
            triggered = true;
        }

    }
}
