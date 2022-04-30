using UnityEngine;
using Mirror;
using Unity.VisualScripting;

/// <summary>
/// Helper component to give client authority over an object when picked up
/// </summary>
public class NetworkItem : NetworkBehaviour
{
    [DoNotSerialize]
    public ItemType ItemType = ItemType.Weapon;

    GameObject owner;

    public GameObject Owner => owner;

    public Transform OwnerInteractAimTransform => Util.GetPlayerInteractAimTransform(this.Owner);

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
            if(_activated)
            {
                EventBus.Trigger(nameof(EventItemActivated), this.gameObject);
            }
            else
            {
                EventBus.Trigger(nameof(EventItemDeactivated), this.gameObject);
            }
        }
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
        if(this.isClientOnly)
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

        if(actor.connectionToClient != null)
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
        if(owner != null && owner.activeSelf)
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
        if (itemOwner  == null || itemOwner.CanPickup(this) == false)
            return;
        
        Pickup(actor);
    }


    [ClientRpc(includeOwner = false)]
    void RpcPrimaryAttackFired()
    {
        EventBus.Trigger(nameof(EventPrimaryAttackFired), this.gameObject);
    }

    [Command]
    void CmdPrimaryAttackFired()
    {
        RpcPrimaryAttackFired();
    }

    // Call to trigger "On Primary Attack Fired" in Visual scripting
    [Client]
    public void OnPrimaryAttackFired()
    {
        EventBus.Trigger(nameof(EventPrimaryAttackFired), this.gameObject);
        CmdPrimaryAttackFired();
    }


    [ClientRpc(includeOwner = false)]
    void RpcSecondaryAttackFired()
    {
        EventBus.Trigger(nameof(EventSecondaryAttackFired), this.gameObject);
    }

    [Command]
    void CmdSecondaryAttackFired()
    {
        RpcSecondaryAttackFired();
    }

    // Call to trigger "On Secondary Attack Fired" in Visual scripting
    [Client]
    public void OnSecondaryAttackFired()
    {
        EventBus.Trigger(nameof(EventSecondaryAttackFired), this.gameObject);
        CmdPrimaryAttackFired();
    }


    public void OnPrimaryAttack(bool wasPressed)
    {
        if (!Activated)
            return;

        if(wasPressed)
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