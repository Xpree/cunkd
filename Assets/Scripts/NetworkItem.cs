using UnityEngine;
using Mirror;
using Unity.VisualScripting;

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



[UnitTitle("On Player Interact")]
[UnitCategory("Events\\Player Actions")]
public class EventPlayerInteract: GameObjectEventUnit<NetworkIdentity>
{
    public override System.Type MessageListenerType => null;

    protected override string hookName => nameof(EventPlayerInteract);

    [DoNotSerialize]// No need to serialize ports.
    public ValueOutput interactingEntity { get; private set; }// The event output data to return when the event is triggered.

    protected override void Definition()
    {
        base.Definition();
        // Setting the value on our port.
        interactingEntity = ValueOutput<NetworkIdentity>(nameof(interactingEntity));
    }

    // Setting the value on our port.
    protected override void AssignArguments(Flow flow, NetworkIdentity data)
    {
        flow.SetValue(interactingEntity, data);
    }
}




[UnitTitle("On Primary Attack Pressed")]
[UnitCategory("Events\\Network Item")]
public class EventPrimaryAttackPressed : GameObjectEventUnit<EmptyEventArgs>
{
    public override System.Type MessageListenerType => null;

    protected override string hookName => nameof(EventPrimaryAttackPressed);
}


[UnitTitle("On Primary Attack Released")]
[UnitCategory("Events\\Network Item")]
public class EventPrimaryAttackReleased : GameObjectEventUnit<EmptyEventArgs>
{
    public override System.Type MessageListenerType => null;

    protected override string hookName => nameof(EventPrimaryAttackReleased);
}


[UnitTitle("On Secondary Attack Pressed")]
[UnitCategory("Events\\Network Item")]
public class EventSecondaryAttackPressed : GameObjectEventUnit<EmptyEventArgs>
{
    public override System.Type MessageListenerType => null;

    protected override string hookName => nameof(EventSecondaryAttackPressed);
}


[UnitTitle("On Secondary Attack Release")]
[UnitCategory("Events\\Network Item")]
public class EventSecondaryAttackReleased : GameObjectEventUnit<EmptyEventArgs>
{
    public override System.Type MessageListenerType => null;

    protected override string hookName => nameof(EventSecondaryAttackReleased);
}


[UnitTitle("On Item Holstered")]
[UnitCategory("Events\\Network Item")]
public class EventItemHolstered : GameObjectEventUnit<EmptyEventArgs>
{
    public override System.Type MessageListenerType => null;

    protected override string hookName => nameof(EventItemHolstered);
}

[UnitTitle("On Item Unholstered")]
[UnitCategory("Events\\Network Item")]
public class EventItemUnholstered : GameObjectEventUnit<EmptyEventArgs>
{
    public override System.Type MessageListenerType => null;

    protected override string hookName => nameof(EventItemUnholstered);
}


[UnitTitle("On Item Picked Up")]
[UnitCategory("Events\\Network Item")]
public class EventItemPickedUp : GameObjectEventUnit<bool>
{
    public override System.Type MessageListenerType => null;

    protected override string hookName => nameof(EventItemPickedUp);

    [DoNotSerialize]// No need to serialize ports.
    public ValueOutput startHolstered { get; private set; }// The event output data to return when the event is triggered.


    protected override void Definition()
    {
        base.Definition();
        // Setting the value on our port.
        startHolstered = ValueOutput<bool>(nameof(startHolstered));
    }

    // Setting the value on our port.
    protected override void AssignArguments(Flow flow, bool data)
    {
        flow.SetValue(startHolstered, data);
    }
}


[UnitTitle("On Item Dropped")]
[UnitCategory("Events\\Network Item")]
public class EventItemDropped : GameObjectEventUnit<EmptyEventArgs>
{
    public override System.Type MessageListenerType => null;

    protected override string hookName => nameof(EventItemDropped);
}