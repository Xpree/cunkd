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

    [Client]
    void destroyIce()
    {
        iceMachine.unFreezeObjects();
        foreach (var item in iceMachine.iceMat)
        {
            Destroy(item);
        }
    }

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
