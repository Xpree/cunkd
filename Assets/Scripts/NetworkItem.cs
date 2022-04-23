using UnityEngine;
using Mirror;

/// <summary>
/// Helper component to give client authority over an object when picked up
/// </summary>
public class NetworkItem : NetworkBehaviour
{
    GameObject owner;

    public GameObject Owner => owner;

    public Transform OwnerInteractAimTransform => Util.GetPlayerInteractAimTransform(this.Owner);

    void OnDropped()
    {
        if(owner != null)
            owner.GetComponent<INetworkItemOwner>()?.OnDropped(this);
    }

    void OnPickedUp()
    {
        if(owner != null)
            owner.GetComponent<INetworkItemOwner>()?.OnPickedUp(this);
    }

    [ClientRpc]
    void RpcDroppedItem()
    {
        if(this.isClientOnly)
        {
            OnDropped();
            owner = null;
        }
    }

    [Command]
    public void CmdDrop()
    {
        this.GetComponent<NetworkIdentity>().RemoveClientAuthority();
        RpcDroppedItem();
        OnDropped();
        owner = null;
    }

    [ClientRpc]
    void RpcPickedUpItem(GameObject actor)
    {
        if(this.isClientOnly)
        {
            owner = actor;
            OnPickedUp();
        }
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

        RpcPickedUpItem(actor.gameObject);
        owner = actor.gameObject;
        OnPickedUp();
    }
}

public interface INetworkItemOwner
{
    /// <summary>
    /// Runs on server and all clients when an item is dropped
    /// </summary>
    /// <param name="item"></param>
    void OnDropped(NetworkItem item);

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
