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

    void OnChangedOwner(GameObject actor)
    {
        owner = actor;
        if (owner != null)
            owner.GetComponent<INetworkItemOwner>()?.OnPickedUp(this);
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
