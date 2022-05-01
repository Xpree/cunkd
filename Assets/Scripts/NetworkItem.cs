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

    public Transform OwnerInteractAimTransform => Util.GetPlayerInteractAimTransform(this.Owner);

    [Tooltip("Defaults to OwnerInteractAimTransform if set to None")]
    [SerializeField] Transform itemAimTransform;

    public Transform AimTransform => itemAimTransform == null ? OwnerInteractAimTransform : itemAimTransform;

    public Ray AimRay => this.AimTransform.ForwardRay();

    bool _activated = false;
    public bool Activated
    {
        get
        {
            return _activated;
        }
        set
        {
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

    public void OnPrimaryAttack(bool wasPressed)
    {
        if (!Activated)
            return;

        if (wasPressed)
            EventBus.Trigger(nameof(EventPrimaryAttackPressed), this.gameObject);
        else
            EventBus.Trigger(nameof(EventPrimaryAttackReleased), this.gameObject);
    }

    public void OnSecondaryAttack(bool wasPressed)
    {
        if (!Activated)
            return;

        if (wasPressed)
            EventBus.Trigger(nameof(EventSecondaryAttackPressed), this.gameObject);
        else
            EventBus.Trigger(nameof(EventSecondaryAttackReleased), this.gameObject);
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