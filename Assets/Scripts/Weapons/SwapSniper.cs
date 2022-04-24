using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkItem))]
public class SwapSniper : NetworkBehaviour, IWeapon, IEquipable
{
    [SerializeField] GameSettings _settings;
    float cooldown => _settings.SwapSniper.Cooldown;
    float range => _settings.SwapSniper.Range;

    [SerializeField] LayerMask TargetMask = ~0;

    [SyncVar] NetworkTimer _nextShotTimer;

    private void Start()
    {
        if (_settings == null)
        {
            Debug.LogError("Missing GameSettings reference on " + name);
        }

    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        _nextShotTimer = NetworkTimer.Now;
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
        if (target == null || _nextShotTimer.HasTicked == false)
            return;
        _nextShotTimer = NetworkTimer.FromNow(cooldown);

        var owner = GetComponent<NetworkItem>()?.Owner;
        if (owner == null)
            return;

        Vector3 Swapper = owner.transform.position;
        Vector3 Swappee = target.transform.position;

        Util.Teleport(target.gameObject, Swapper);
        Util.Teleport(owner.gameObject, Swappee);
    }

    void IWeapon.PrimaryAttack(bool isPressed)
    {
        if(isPressed)
        {
            if(_nextShotTimer.HasTicked)
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

    void IEquipable.OnRemoved()
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
