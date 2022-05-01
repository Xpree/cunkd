using UnityEngine;
using Mirror;
using Unity.VisualScripting;

/// <summary>
/// Helper component to give client authority over an object when picked up
/// </summary>
public class NetworkItem : NetworkBehaviour
{
    [SerializeField] string displayName;
    [SerializeField] Transform rotationCenter;
    public Collider PickupColldider;

    public string DisplayName => string.IsNullOrEmpty(displayName) ? this.gameObject.name : displayName;

    public Transform RotationCenter => rotationCenter == null ? this.transform : rotationCenter;

    [HideInInspector]
    public ItemType ItemType = ItemType.Weapon;

    GameObject owner;

    public GameObject Owner => owner;

    public T GetOwnerComponent<T>()
    {
        if (owner == null || owner.activeSelf == false)
        {
            return default(T);
        }
        return owner.GetComponent<T>();
    }

    public Transform OwnerInteractAimTransform => Util.GetPlayerInteractAimTransform(this.Owner);

    [Tooltip("Defaults to OwnerInteractAimTransform if set to None")]
    [SerializeField] Transform itemAimTransform;

    public Transform AimTransform => itemAimTransform == null ? OwnerInteractAimTransform : itemAimTransform;

    public Ray AimRay => this.AimTransform.ForwardRay();


    bool _primaryAttack;
    bool _secondaryAttack;
    bool _activated = false;

    void SetPrimaryAttack(bool pressed)
    {
        _primaryAttack = pressed;
        if (_activated)
        {
            if (pressed)
                EventBus.Trigger(nameof(EventPrimaryAttackPressed), this.gameObject);
            else
                EventBus.Trigger(nameof(EventPrimaryAttackReleased), this.gameObject);
        }
    }

    [ClientRpc(includeOwner = false)]
    void RpcPrimaryPressed(bool pressed)
    {
        SetPrimaryAttack(pressed);
    }

    [Command]
    void CmdPrimaryPressed(bool pressed)
    {
        if(isServerOnly)
            SetPrimaryAttack(pressed);
        RpcPrimaryPressed(pressed);
    }

    void SetSecondaryAttack(bool pressed)
    {
        _secondaryAttack = pressed;
        if (_activated)
        {
            if (pressed)
                EventBus.Trigger(nameof(EventSecondaryAttackPressed), this.gameObject);
            else
                EventBus.Trigger(nameof(EventSecondaryAttackReleased), this.gameObject);
        }
    }


    [ClientRpc(includeOwner = false)]
    void RpcSecondaryPressed(bool pressed)
    {
        SetSecondaryAttack(pressed);
    }

    [Command]
    void CmdSecondaryPressed(bool pressed)
    {
        if (isServerOnly)
            SetSecondaryAttack(pressed);
        RpcSecondaryPressed(pressed);
    }

    public bool PrimaryAttackPressed => _activated && _primaryAttack;
    public bool SecondaryAttackPressed => _activated && _secondaryAttack;

    public bool AnyAttackPressed => _activated && (_primaryAttack || _secondaryAttack);

    public void OnPrimaryAttack(bool wasPressed)
    {
        if(wasPressed != _primaryAttack)
        {
            SetPrimaryAttack(wasPressed);
            CmdPrimaryPressed(wasPressed);            
        }
    }

    public void OnSecondaryAttack(bool wasPressed)
    {
        if (wasPressed != _secondaryAttack)
        {
            SetSecondaryAttack(wasPressed);
            CmdSecondaryPressed(wasPressed);            
        }
    }

    public bool Activated
    {
        get
        {
            return _activated;
        }
        set
        {
            if (_activated == value)
                return;

            if(!value)
            {
                if (_primaryAttack)
                    SetPrimaryAttack(false);
                if (_secondaryAttack)
                    SetSecondaryAttack(false);
            }

            _activated = value;
            if (_activated)
            {
                EventBus.Trigger(nameof(EventItemActivated), this.gameObject);
            }
            else
            {
                EventBus.Trigger(nameof(EventItemDeactivated), this.gameObject);
            }
        }
    }

    public Vector3 RaycastPointOrMaxDistance(float maxDistance, LayerMask layerMask)
    {
        var aimRay = this.AimRay;

        if (Physics.Raycast(aimRay, out RaycastHit hit, maxDistance, layerMask, QueryTriggerInteraction.Ignore))
        {
            return hit.point;
        }
        else
        {
            return aimRay.GetPoint(maxDistance);
        }
    }

    public Vector3 SphereCastPointOrMaxDistance(float maxDistance, LayerMask layerMask, float radius)
    {
        var aimRay = this.AimRay;

        if (Physics.SphereCast(aimRay, radius, out RaycastHit hit, maxDistance, layerMask, QueryTriggerInteraction.Ignore))
        {
            return hit.point;
        }
        else
        {
            return aimRay.GetPoint(maxDistance);
        }
    }

    public NetworkIdentity SphereCastNetworkIdentity(float maxDistance, LayerMask layerMask, float radius)
    {
        var aimRay = this.AimRay;

        if (Physics.SphereCast(aimRay, radius, out RaycastHit hit, maxDistance, layerMask, QueryTriggerInteraction.Ignore))
        {
            return hit.collider.GetComponent<NetworkIdentity>();
        }
        else
        {
            return null;
        }
    }


    public bool PickupEnabled
    {
        get { return PickupColldider.enabled; }
        set { PickupColldider.enabled = value; }
    }

    public void SetPositionWithRotationCenter(Transform target)
    {
        this.transform.position = target.position;
        this.transform.position = target.position - (RotationCenter.position - target.position);
    }

    private void Awake()
    {
        if (PickupColldider == null)
            Debug.LogError("Missing Pickup colldier on: " + this.name);
    }

    void OnChangedOwner(GameObject actor)
    {
        owner = actor;
        if (owner != null)
        {
            owner.GetComponent<INetworkItemOwner>()?.OnPickedUp(this);
        }

    }

    [ClientRpc]
    void RpcChangedOwner(GameObject actor)
    {
        if (this.isClientOnly)
        {
            OnChangedOwner(actor);
        }
    }

    [Server]
    void ChangeOwner(GameObject actor)
    {
        RpcChangedOwner(actor);
        OnChangedOwner(actor);
    }

    [Server]
    public void Pickup(NetworkIdentity actor)
    {
        if (actor?.GetComponent<INetworkItemOwner>() == null)
        {
            Debug.LogError("GameObject is not a NetworkItemOwner");
            return;
        }

        if (owner != null)
        {
            Debug.LogError("NetworkItem is already picked up.");
            return;
        }

        if (actor.connectionToClient != null)
        {
            this.GetComponent<NetworkIdentity>().AssignClientAuthority(actor.connectionToClient);
        }

        ChangeOwner(actor.gameObject);
    }

    [Command]
    public void CmdDropOwnership()
    {
        this.GetComponent<NetworkIdentity>().RemoveClientAuthority();
        ChangeOwner(null);
    }

    private void OnDestroy()
    {
        if (owner != null && owner.activeSelf)
        {
            owner.GetComponent<INetworkItemOwner>()?.OnDestroyed(this);
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdTryPickup(NetworkIdentity actor)
    {
        if (Owner != null)
            return;

        var itemOwner = actor.GetComponent<INetworkItemOwner>();
        if (itemOwner == null || itemOwner.CanPickup(this) == false)
            return;

        Pickup(actor);
    }

}

public interface INetworkItemOwner
{
    /// <summary>
    /// Runs on server and all clients when an item is destroyed
    /// </summary>
    /// <param name="item"></param>
    void OnDestroyed(NetworkItem item);

    /// <summary>
    /// Runs on server and all clients when an item is picked up
    /// </summary>
    /// <param name="item"></param>
    void OnPickedUp(NetworkItem item);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <returns>Return true if item can be picked up</returns>
    bool CanPickup(NetworkItem item);
}

[System.Serializable]
public enum ItemType
{
    Weapon,
    Gadget
}