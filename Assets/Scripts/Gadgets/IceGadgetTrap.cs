using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class IceGadgetTrap : NetworkBehaviour
{
    [SerializeField] GameSettings _settings;
    [SyncVar] NetworkTimer _endTime;

    public float friction => _settings.IceGadget.Friction;

    [SyncVar(hook = nameof(OnParent))] public Transform parentSync;

    [SerializeField] public SpreadMat iceMachine;
    //public IceGadget owner;
    public bool owner = false;
    public IceTrapHub hub;
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
        if(player.Invulnerabiliy() == false)
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/SoundStudents/SFX/Gadgets/WalkingOnIcyFloor2D");
            player.GetComponent<PlayerMovement>().maxFrictionScaling = friction;
            player.GetComponent<PlayerMovement>().maxSpeedScaling = 0.1f;
        }
    }

    //[Command]
    void setParent(Transform parent)
    {
        parentSync = parent;
    }

    [Client]
    void OnParent(Transform previous, Transform current)
    {
        this.transform.SetParent(current);
        hub.transform.SetParent(this.transform);
    }

    bool triggered = false;
    //[Server]
    private void OnCollisionEnter(Collision collision)
    {
        if (owner)
        {
            if (owner && !triggered && collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                NetworkIdentity parent = collision.gameObject.GetComponentInParent<NetworkIdentity>();
                if (parent)
                {
                    setParent(parent.transform);
                    this.transform.SetParent(parent.transform);
                    hub.setParent(parent.transform);
                    hub.transform.SetParent(parent.transform);
                }
                hub.sync(transform.position);
                GetComponent<Rigidbody>().isKinematic = true;
                triggered = true;
            }
        }
    }
}