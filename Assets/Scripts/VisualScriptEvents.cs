using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using Mirror;


[UnitTitle("On Network Cooldown Begin")]
[UnitCategory("Events\\Network Cooldown")]
public class EventNetworkCooldownBegin : GameObjectEventUnit<EmptyEventArgs>
{
    public override System.Type MessageListenerType => null;

    protected override string hookName => nameof(EventNetworkCooldownBegin);
}


[UnitTitle("On Network Cooldown End")]
[UnitCategory("Events\\Network Cooldown")]
public class EventNetworkCooldownEnd : GameObjectEventUnit<EmptyEventArgs>
{
    public override System.Type MessageListenerType => null;

    protected override string hookName => nameof(EventNetworkCooldownEnd);
}

[UnitTitle("On Player Interact")]
[UnitCategory("Events\\Player Actions")]
public class EventPlayerInteract : GameObjectEventUnit<NetworkIdentity>
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


[UnitTitle("On Gun Fired")]
[UnitCategory("Events\\Network Item")]
public class EventGunFired : GameObjectEventUnit<EmptyEventArgs>
{
    public override System.Type MessageListenerType => null;

    protected override string hookName => nameof(EventGunFired);
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