using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class IceGadgetTrap : NetworkBehaviour
{
    [SerializeField] GameSettings _settings;
    [SerializeField] public SpreadMat iceMachine;
    
    [SyncVar] NetworkTimer _endTime;
    public float friction => _settings.IceGadget.Friction;

    public int index = 0;
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

    bool triggered = false;
    private void OnCollisionEnter(Collision collision)
    {
        if (owner)
        {
            if (owner && !triggered && collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                triggered = true;
                GetComponent<Rigidbody>().isKinematic = true;
                iceMachine.Trigger();

                Transform parent = null;
                NetworkIdentity CollisionParent = collision.gameObject.GetComponentInParent<NetworkIdentity>();
                if (CollisionParent)
                {
                    if (CollisionParent.transform.CompareTag("Platform"))
                    {
                        parent = CollisionParent.transform;
                        transform.SetParent(parent, true);
                    }
                }
                hub.sync(index, transform.position, parent);

            }
        }
    }
}