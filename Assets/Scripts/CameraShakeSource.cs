using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.InputSystem;

public class CameraShakeSource : MonoBehaviour
{
    public CameraShake cameraShake;
    public float ActivationRange = 2.0f;
    public bool IsShaking => cameraShake.activationTimer.Elapsed < cameraShake.Duration;


    public Key testingKey = Key.None;

    public void OneShotShake(NetworkTimer eventTime)
    {
        cameraShake = (CameraShake)cameraShake.Clone();
        cameraShake.Initialize(eventTime);
        
        var layerMask = GameServer.Instance.Settings.ShakeableLayer;
        foreach(var collider in Physics.OverlapSphere(this.transform.position, ActivationRange, layerMask, QueryTriggerInteraction.Collide))
        {
            EventBus.Trigger(new EventHook(nameof(EventCameraShake), collider.gameObject), this);
        }
    }

    private void Update()
    {
        if (testingKey != Key.None && Keyboard.current[testingKey].wasPressedThisFrame)
        {
            Debug.Log("Testing Shake from " + this.name);
            OneShotShake(NetworkTimer.Now);
        }
    }
}

[UnitTitle("On Camera Shake")]
[UnitCategory("Events\\Camera")]
public class EventCameraShake : GameObjectEventUnit<CameraShakeSource>
{
    public override System.Type MessageListenerType => null;

    protected override string hookName => nameof(EventCameraShake);

    [DoNotSerialize]// No need to serialize ports.
    public ValueOutput source { get; private set; }// The event output data to return when the event is triggered.

    protected override void Definition()
    {
        base.Definition();
        // Setting the value on our port.
        source = ValueOutput<CameraShakeSource>(nameof(source));
    }

    // Setting the value on our port.
    protected override void AssignArguments(Flow flow, CameraShakeSource data)
    {
        flow.SetValue(source, data);
    }
}