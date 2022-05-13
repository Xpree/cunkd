using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class IceGadgetTrap : NetworkBehaviour
{
    [SerializeField] GameSettings _settings;
    [SyncVar] NetworkTimer _endTime;

    [SyncVar(hook = nameof(OnTriggered))] bool triggeredSync = false;
    [SyncVar(hook = nameof(OnPosition))] Vector3 positionSync;
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

    void OnPosition(Vector3 previous, Vector3 current)
    {
        transform.position = current;
        triggered = false;
    }

    [Client]
    public void OnTriggered(bool previous, bool current)
    {
        transform.position = positionSync;
        triggered = false;
    }

    public void makePlayerSlip(GameObject player)
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/SoundStudents/SFX/Gadgets/WalkingOnIcyFloor2D");
        player.GetComponent<PlayerMovement>().maxFrictionScaling = friction;
        player.GetComponent<PlayerMovement>().maxSpeedScaling = 0f;
    }

    private void Update()
    {
        if (!triggered)
        {
            iceMachine.Trigger();
            triggered = true;
        }
    }

    //[Client]
    //void destroyIce()
    //{
    //    iceMachine.unFreezeObjects();
    //    foreach (var item in iceMachine.iceMat)
    //    {
    //        Destroy(item);
    //    }
    //}

    bool triggered = true;
    [Server]
    private void OnCollisionEnter(Collision collision)
    {
        if (!triggeredSync && triggered && collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            positionSync = transform.position;
            GetComponent<Rigidbody>().isKinematic = true;
            //iceMachine.Trigger();
            //triggeredSync = true;
            //triggered = true;
        }

    }
}
