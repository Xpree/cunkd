using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkItem))]
[RequireComponent(typeof(NetworkCooldown))]
public class GadgetExampleBanana : NetworkBehaviour, IGadget, IEquipable
{
    [SerializeField] bool isPassive;
    [SerializeField] int Charges;
    [SerializeField] float Cooldown = 1.0f;

    NetworkCooldown cooldownTimer;

    bool IGadget.isPassive => isPassive;
    int IGadget.Charges => Charges;
    int IGadget.ChargesLeft => cooldownTimer.Charges;

    private void Awake()
    {
        cooldownTimer = GetComponent<NetworkCooldown>();
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
        if (cooldownTimer.Use(this.Cooldown))
        {
            TargetTell("ate a piece of the banana");
            if (cooldownTimer.Charges == 0)
            {
                TargetTell("out of banana");
                NetworkServer.Destroy(this.gameObject);
                return;
            }
        }
        else
        {
            // Use attempt failed. Let client know it still has charges
            cooldownTimer.ForceUpdateClients();
        }
    }

    void IGadget.PrimaryUse(bool isPressed)
    {
        if(cooldownTimer.Use(this.Cooldown))
        {
            CmdUse();
        }
    }

    void IGadget.SecondaryUse(bool isPressed)
    {
        if (cooldownTimer.Use(this.Cooldown))
        {
            CmdUse();
        }
    }

    float? IGadget.ChargeProgress => null;


    bool holstered;
    bool IEquipable.IsHolstered => holstered;

    void IEquipable.OnHolstered()
    {
        // TODO Animation then set holstered
        holstered = true;
        transform.localScale = Vector3.zero;
    }

    void IEquipable.OnUnholstered()
    {
        // TODO Animation then set holstered
        holstered = false;
        transform.localScale = Vector3.one;
    }

    void IEquipable.OnPickedUp(bool startHolstered)
    {
        holstered = startHolstered;

        if (holstered)
            transform.localScale = Vector3.zero;
        else
            transform.localScale = Vector3.one;
    }

    void IEquipable.OnDropped()
    {
        this.transform.parent = null;
        if (holstered)
        {
            holstered = false;
            transform.localScale = Vector3.one;
        }
    }
}
