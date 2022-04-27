using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.VFX;

[RequireComponent(typeof(NetworkItem))]
[RequireComponent(typeof(NetworkCooldown))]
public class SwapSniper : NetworkBehaviour, IWeapon, IEquipable
{
    [SerializeField] GameObject teleportVFX;
    [SerializeField] GameSettings _settings;
    float cooldown => _settings.SwapSniper.Cooldown;
    float range => _settings.SwapSniper.Range;

    [SerializeField] LayerMask TargetMask = ~0;

    NetworkCooldown _cooldownTimer;
    
    void Awake()
    {
        _cooldownTimer = GetComponent<NetworkCooldown>();
        _cooldownTimer.coolDownDuration = cooldown;
    }

    private void Start()
    {
        if (_settings == null)
        {
            Debug.LogError("Missing GameSettings reference on " + name);
        }
    }

    public NetworkIdentity DidHitObject()
    {
        var aimTransform = Util.GetOwnerAimTransform(GetComponent<NetworkItem>());
        if (Physics.Raycast(aimTransform.position, aimTransform.forward, out RaycastHit hitResult, range, TargetMask))
        {            
            return hitResult.rigidbody?.GetComponent<NetworkIdentity>();
        }
        else
        {
            return null;
        }
    }

    [Command]
    void CmdPerformSwap(NetworkIdentity target)
    {
        if (target == null || _cooldownTimer.ServerUse(this.cooldown) == false)
        {
            // Client predicted wrong. Dont care!
            return;
        }

        var owner = GetComponent<NetworkItem>()?.Owner;
        if (owner == null)
            return;

        Vector3 Swapper = owner.transform.position;
        Vector3 Swappee = target.transform.position;
        
        teleportVFX.GetComponent<VisualEffect>().Play();
        Util.Teleport(target.gameObject, Swapper);
        Util.Teleport(owner.gameObject, Swappee);

    }

    void IWeapon.PrimaryAttack(bool isPressed)
    {
        if(isPressed)
        {
            if(_cooldownTimer.Use(this.cooldown))
            {
                CmdPerformSwap(DidHitObject());
            }
        }
    }

    void IWeapon.SecondaryAttack(bool isPressed)
    {

    }

    float? IWeapon.ChargeProgress => null;


    #region IEquipable

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
    #endregion
}
