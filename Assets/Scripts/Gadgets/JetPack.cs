using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.VFX;

[RequireComponent(typeof(NetworkItem))]
[RequireComponent(typeof(NetworkCooldown))]
public class JetPack : NetworkBehaviour, IGadget, IEquipable
{
    [SerializeField] NetworkAnimator animator;

    [SerializeField] bool isPassive;
    [SerializeField] int Charges;
    [SerializeField] float Cooldown = 0.05f;
    [SerializeField] float maxForce = 1.0f;
    [SerializeField] float acceleration = 0.01f;

    NetworkCooldown cooldownTimer;

    bool IGadget.isPassive => isPassive;
    int IGadget.Charges => Charges;
    int IGadget.ChargesLeft => cooldownTimer.Charges;

    NetworkItem item;

    private void Awake()
    {
        cooldownTimer = GetComponent<NetworkCooldown>();
        cooldownTimer.SetCharges(Charges);
        cooldownTimer.CooldownDuration = Cooldown;
        
        item = GetComponent<NetworkItem>();
    }

    float force = 0;

    [Command]
    void CmdUseCharge()
    {
        if (cooldownTimer.ServerUse(this.Cooldown))
        {
            if (cooldownTimer.Charges == 0)
            {
                NetworkServer.Destroy(this.gameObject);
                return;
            }
        }
    }

    void SetFlying(bool enable)
    {
        if(enable)
        {
            if (!isFlying)
            {
                animator.SetTrigger("Fly");
            }
        }
        else
        {
            timeToFly = false;
            if (isFlying)
            {
                animator.SetTrigger("StopFly");
            }
        }
        isFlying = enable;
    }

    bool timeToFly = false;
    bool isFlying = false;

    private void FixedUpdate()
    {
        if (item.IsOwnerLocalPlayer == false)
            return;

        // Only ran by local player
        
        if (timeToFly && cooldownTimer.Charges > 0)
        {            
            if(cooldownTimer.Use(this.Cooldown))
            {
                CmdUseCharge();
                force = Mathf.Min(force += acceleration, maxForce);
                PlayerMovement pm = GetComponentInParent<PlayerMovement>();
                pm.ApplyJumpForce(force);
                SetFlying(true);
            }
        }
        else
        {
            SetFlying(false);
        }
    }

    void IGadget.PrimaryUse(bool isPressed)
    {
        //AudioHelper.PlayOneShotAttachedWithParameters("event:/SoundStudents/SFX/Gadgets/Jetpack", this.gameObject, ("Gasar", 1f), ("Br�nsle", 0f),
        //                                     ("Ta p� jetpack", 1f), ("Jetpack flyger tomg�ng", 1f), ("Jetpack st�ngs av", 0f));
        
        
        force = 0;
        timeToFly = isPressed;
    }

    void IGadget.SecondaryUse(bool isPressed)
    {
        force = 0;
        timeToFly = isPressed;
    }

    float? IGadget.ChargeProgress => null;


    bool holstered;
    bool IEquipable.IsHolstered => holstered;

    void IEquipable.OnHolstered()
    {
        // TODO Animation then set holstered
        holstered = true;
        transform.localScale = Vector3.zero;

        if(item.IsOwnerLocalPlayer)
            SetFlying(false);
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
        if (item.IsOwnerLocalPlayer)
            SetFlying(false);

        this.transform.parent = null;
        if (holstered)
        {
            holstered = false;
            transform.localScale = Vector3.one;
        }
    }
}
