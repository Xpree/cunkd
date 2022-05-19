using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class IceGadgetTrap : NetworkBehaviour
{
    [SerializeField] GameSettings _settings;
    [SyncVar] NetworkTimer _endTime;

    public float friction => _settings.IceGadget.Friction;

    [SerializeField] public SpreadMat iceMachine;
    public IceGadget owner;
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
        FMODUnity.RuntimeManager.PlayOneShot("event:/SoundStudents/SFX/Gadgets/WalkingOnIcyFloor2D");
        player.GetComponent<PlayerMovement>().maxFrictionScaling = friction;
        player.GetComponent<PlayerMovement>().maxSpeedScaling = 0f;
    }

    bool triggered = false;
    //[Server]
    private void OnCollisionEnter(Collision collision)
    {
        if (owner)
        {
            if (owner && !triggered && collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                owner.sync(transform.position);
                GetComponent<Rigidbody>().isKinematic = true;
                triggered = true;
            }
        }
    }
}