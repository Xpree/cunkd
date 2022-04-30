using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkItem))]
[RequireComponent(typeof(NetworkCooldown))]
public class GadgetExampleBanana : NetworkBehaviour
{
    [SerializeField] int Charges;
    [SerializeField] float Cooldown = 1.0f;

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

    [Command]
    void CmdUse()
    {
        if (cooldownTimer.ServerUse())
        {
            TargetTell("ate a piece of the banana");
            if (cooldownTimer.Charges == 0)
            {
                TargetTell("out of banana");
                NetworkServer.Destroy(this.gameObject);
                return;
            }
        }
    }

    public bool Use()
    {
        if(cooldownTimer.Use())
        {
            CmdUse();
            return true;
        }
        else
        {
            return false;
        }        
    }
}
