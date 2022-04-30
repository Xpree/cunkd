using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.VFX;

[RequireComponent(typeof(NetworkItem))]
[RequireComponent(typeof(NetworkCooldown))]
public class JetPack : NetworkBehaviour
{
    [SerializeField] bool isPassive;
    [SerializeField] int Charges;
    [SerializeField] float Cooldown = 0.05f;
    [SerializeField] float maxForce = 1.0f;
    [SerializeField] float acceleration = 0.01f;

    NetworkCooldown cooldownTimer;
    NetworkItem item;

    private void Awake()
    {
        item = GetComponent<NetworkItem>();
        item.ItemType = ItemType.Gadget;

        cooldownTimer = GetComponent<NetworkCooldown>();
        cooldownTimer.CooldownDuration = Cooldown;
        cooldownTimer.MaxCharges = Charges;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        cooldownTimer.SetCharges(Charges);
    }

    [TargetRpc]
    void TargetTell(string message)
    {
        print(message);
    }

    float force = 0;

    [Command]
    void fly()
    {
        if (cooldownTimer.ServerUse(this.Cooldown))
        {
            force = Mathf.Min(force += acceleration, maxForce);
            RpcFlyLikeSatan();
            //animator.SetTrigger("Fly");
            if (cooldownTimer.Charges == 0)
            {
                TargetTell("out of fuel");
                NetworkServer.Destroy(this.gameObject);
                return;
            }
        }
    }

    bool timeToFly = false;
    private void FixedUpdate()
    {
        if (timeToFly)
        {
            fly();
        }
        else
        {
            //animator.SetTrigger("StopFly");
        }
    }

    [TargetRpc]
    void RpcFlyLikeSatan()
    {
        //print("Look mom I'm flying!");
        force = Mathf.Min(force += acceleration, maxForce);
        PlayerMovement pm = GetComponentInParent<PlayerMovement>();
        pm.ApplyJumpForce(force);
    }


    public bool StartFlying()
    {
        return false;
    }

}
