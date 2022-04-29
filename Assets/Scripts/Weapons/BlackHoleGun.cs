using System.Collections;
using UnityEngine;
using Mirror;
using Unity.VisualScripting;

[RequireComponent(typeof(NetworkItem))]
[RequireComponent(typeof(NetworkCooldown))]
public class BlackHoleGun : NetworkBehaviour, IWeapon
{    
    [SerializeField] GameSettings _settings;
    public float Cooldown => _settings.BlackHoleGun.Cooldown;
    public float MaxRange => _settings.BlackHoleGun.Range;

    [SerializeField] GameObject blackHole;
    [SerializeField] public LayerMask TargetMask = ~0;

    NetworkCooldown _cooldownTimer;
    void Awake()
    {
        _cooldownTimer = GetComponent<NetworkCooldown>();
        _cooldownTimer.coolDownDuration = Cooldown;
    }

    private void Start()
    {
        if (_settings == null)
        {
            Debug.LogError("Missing GameSettings reference on " + name);
        }
    }

    [Command]
    public void CmdSpawnBlackHole(Vector3 target)
    {
        if (_cooldownTimer.ServerUse())
        {
            var go = Instantiate(blackHole, target, Quaternion.identity);
            NetworkServer.Spawn(go);
            RpcGunFired();
        }
    }

    [ClientRpc(includeOwner = false)]
    void RpcGunFired()
    {
        EventBus.Trigger(nameof(EventGunFired), this.gameObject);
    }

    public bool Shoot()
    {
        if(_cooldownTimer.Use())
        {
            var aim = Util.GetOwnerAimTransform(GetComponent<NetworkItem>());
            var target = Util.RaycastPointOrMaxDistance(aim, MaxRange, TargetMask);
            EventBus.Trigger(nameof(EventGunFired), this.gameObject);
            CmdSpawnBlackHole(target);
            return true;
        }
        else
        {
            return false;
        }
    }

    void IWeapon.PrimaryAttack(bool isPressed)
    {
        
    }

    void IWeapon.SecondaryAttack(bool isPressed)
    {
    }

    float? IWeapon.ChargeProgress => null;
}


[UnitTitle("On Gun Fired")]
[UnitCategory("Events\\Network Item")]
public class EventGunFired : GameObjectEventUnit<EmptyEventArgs>
{
    public override System.Type MessageListenerType => null;

    protected override string hookName => nameof(EventGunFired);
}