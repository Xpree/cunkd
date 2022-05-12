using UnityEngine;
using Mirror;
using Unity.VisualScripting;

/// <summary>
/// Helper component to give client authority over an object when picked up
/// </summary>
public class NetworkItem : NetworkBehaviour
{
    public string displayName;
    public Collider interactCollider;

    GameObject owner;

    public bool IsSpawned { get; private set; }

    public GameObject Owner => owner;

    public Transform OwnerInteractAimTransform => Util.GetPlayerInteractAimTransform(this.Owner);

    public Ray AimRay => this.transform.ForwardRay();

    public T GetOwnerComponent<T>()
    {
        if (owner == null || owner.activeSelf == false)
        {
            return default(T);
        }
        return owner.GetComponent<T>();
    }


    // Called by all clients
    public void OnSpawned(Transform anchor)
    {
        transform.parent = anchor;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = this.transform.position - interactCollider.transform.position;
        transform.localPosition += Vector3.up * interactCollider.bounds.extents.y;
        interactCollider.enabled = true;
        IsSpawned = true;
    }


    private void OnEnable()
    {        
        if(interactCollider == null)
        {
            Debug.LogError("Missing interact collider on " + this.name);
            return;
        }

        EventBus.Register(new EventHook(nameof(EventPlayerInteract), interactCollider.gameObject), new System.Action<NetworkIdentity>(OnInteract));
        EventBus.Register(new EventHook(nameof(EventPlayerInteractHoverStart), interactCollider.gameObject), new System.Action<NetworkIdentity>(OnInteractHoverStart));
        EventBus.Register(new EventHook(nameof(EventPlayerInteractHoverStop), interactCollider.gameObject), new System.Action<NetworkIdentity>(OnInteractHoverStop));

    }

    private void OnDisable()
    {
        EventBus.Unregister(new EventHook(nameof(EventPlayerInteract), interactCollider.gameObject), new System.Action<NetworkIdentity>(OnInteract));
        EventBus.Unregister(new EventHook(nameof(EventPlayerInteractHoverStart), interactCollider.gameObject), new System.Action<NetworkIdentity>(OnInteractHoverStart));
        EventBus.Unregister(new EventHook(nameof(EventPlayerInteractHoverStop), interactCollider.gameObject), new System.Action<NetworkIdentity>(OnInteractHoverStop));
    }

    void OnInteractHoverStart(NetworkIdentity player)
    {
        FindObjectOfType<PlayerGUI>()?.interactiveItemButton(this);
    }

    void OnInteractHoverStop(NetworkIdentity player)
    {
        FindObjectOfType<PlayerGUI>()?.interactiveItemButton(null);
    }

    public void OnInteract(NetworkIdentity player)
    {
        var itemOwner = player.GetComponent<INetworkItemOwner>();
        if (itemOwner == null || itemOwner.CanPickup(this) == false)
        {
            Debug.Log("TODO: Can't pick up feedback.");
            return;
        }
        
        CmdTryPickup(player);
    }


    void OnChangedOwner(GameObject actor)
    {
        IsSpawned = false;
        
        this.transform.parent = null;
        owner = actor;
        if (owner != null)
        {
            interactCollider.enabled = false;
            owner.GetComponent<INetworkItemOwner>()?.OnPickedUp(this);
        }
        else
        {
            interactCollider.enabled = true;
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
        // this.GetComponent<NetworkIdentity>().RemoveClientAuthority();
        // ChangeOwner(null);
        NetworkServer.Destroy(this.gameObject);
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


    public Vector3 InteractAimPoint(float maxDistance)
    {
        var aimRay = this.OwnerInteractAimTransform.ForwardRay();

        var settings = GameServer.Instance.Settings;

        if (Physics.SphereCast(aimRay, settings.SmallSphereCastRadius, out RaycastHit hit, maxDistance, settings.ProtectileTargetLayers, QueryTriggerInteraction.Ignore))
        {
            return hit.point;
        }
        else
        {
            return aimRay.GetPoint(maxDistance);
        }
    }

    public NetworkIdentity ProjectileHitscanIdentity(float maxDistance)
    {
        return ProjectileHitscanComponent<NetworkIdentity>(maxDistance);
    }

    public T ProjectileHitscanComponent<T>(float maxDistance)where T:class
    {
        var aimRay = this.AimRay;

        var settings = GameServer.Instance.Settings;

        if (Physics.SphereCast(aimRay, settings.SmallSphereCastRadius, out RaycastHit hit, maxDistance, settings.ProtectileTargetLayers, QueryTriggerInteraction.Ignore))
        {
            return hit.collider.GetComponent<T>();
        }
        else
        {
            return null;
        }
    }

    public T ProjectileHitscanOwner<T>(float maxDistance) where T : class
    {
        var aimRay = this.OwnerInteractAimTransform.ForwardRay();

        var settings = GameServer.Instance.Settings;

        if (Physics.SphereCast(aimRay, settings.SmallSphereCastRadius, out RaycastHit hit, maxDistance, settings.ProtectileTargetLayers, QueryTriggerInteraction.Ignore))
        {
            return hit.collider.GetComponent<T>();
        }
        else
        {
            return null;
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
