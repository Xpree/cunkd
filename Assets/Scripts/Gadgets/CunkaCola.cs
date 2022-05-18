using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkItem))]
[RequireComponent(typeof(NetworkCooldown))]
public class CunkaCola : NetworkBehaviour, IGadget, IEquipable
{
    [SerializeField] bool isPassive;
    [SerializeField] int Charges;
    [SerializeField] float Cooldown = 1.0f;
    [SerializeField] float SpeedBoost;
    [SerializeField] float Duration;
    double endTime;

    NetworkCooldown cooldownTimer;

    bool IGadget.isPassive => isPassive;
    int IGadget.Charges => Charges;
    int IGadget.ChargesLeft => cooldownTimer.Charges;

    private void Awake()
    {
        cooldownTimer = GetComponent<NetworkCooldown>();
        cooldownTimer.CooldownDuration = Cooldown;
        cooldownTimer.MaxCharges = Charges;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        cooldownTimer.SetCharges(Charges);
    }

    [Server]
    private void Update()
    {
        if (player && endTime < NetworkTime.time)
        {
            GetComponentInParent<PlayerMovement>().bonusSpeed = 0;
            //TargetTell("Aaaaah!");
            if (cooldownTimer.Charges == 0)
            {
                TargetTell("Buuurp!");
                NetworkServer.Destroy(this.gameObject);
                return;
            }
        }
    }


    [TargetRpc]
    void TargetTell(string message)
    {
        print(message);
    }

    PlayerMovement player;

    [Command]
    void CmdUse()
    {
        if (cooldownTimer.ServerUse(this.Cooldown))
        {
            TargetTell("Drank some Cunka Cola");
            player = GetComponentInParent<PlayerMovement>();
            player.bonusSpeed = SpeedBoost;
            endTime = NetworkTime.time + Duration;
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

        if (player)
        {
            player.bonusSpeed = 0;
        }
        if (holstered)
        {
            holstered = false;
            transform.localScale = Vector3.one;
        }
    }
}
