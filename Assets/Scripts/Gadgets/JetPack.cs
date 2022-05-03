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


    GameInputs gameInputs;
    private void Awake()
    {
        cooldownTimer = GetComponent<NetworkCooldown>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        cooldownTimer.SetCharges(Charges);
        cooldownTimer.CooldownDuration = Cooldown;
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
            animator.SetTrigger("Fly");
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
            animator.SetTrigger("StopFly");
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

    void IGadget.PrimaryUse(bool isPressed)
    {
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
