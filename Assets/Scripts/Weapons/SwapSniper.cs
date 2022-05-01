using UnityEngine;
using Mirror;
using Unity.VisualScripting;

[RequireComponent(typeof(NetworkItem))]
[RequireComponent(typeof(NetworkCooldown))]
public class SwapSniper : NetworkBehaviour
{
    [SerializeField] GameSettings _settings;
    float cooldown => _settings.SwapSniper.Cooldown;
    float range => _settings.SwapSniper.Range;

    [SerializeField] LayerMask TargetMask = ~0;

    NetworkCooldown _cooldownTimer;
    NetworkItem _item;

    void Awake()
    {
        _item = GetComponent<NetworkItem>();
        _item.ItemType = ItemType.Weapon;

        _cooldownTimer = GetComponent<NetworkCooldown>();
        _cooldownTimer.CooldownDuration = cooldown;
    }

    private void Start()
    {
        if (_settings == null)
        {
            Debug.LogError("Missing GameSettings reference on " + name);
        }
    }


    [Command]
    void CmdPerformSwap(NetworkIdentity target)
    {
        if (target == null || _cooldownTimer.ServerUse(this.cooldown) == false)
        {
            // Client predicted wrong. Dont care!
            return;
        }

        var owner = _item.Owner;
        if (owner == null)
            return;

        Vector3 Swapper = owner.transform.position;
        Vector3 Swappee = target.transform.position;

        Util.Teleport(target.gameObject, Swapper);
        Util.Teleport(owner.gameObject, Swappee);
    }

    [Command]
    void CmdTriggerPrimaryAttackFired()
    {
        NetworkEventBus.TriggerExcludeOwner(nameof(EventPrimaryAttackFired), this.netIdentity);
    }


    public bool Shoot()
    {
        if (_cooldownTimer.Use(this.cooldown))
        {
            EventBus.Trigger(nameof(EventPrimaryAttackFired), this.gameObject);
            CmdTriggerPrimaryAttackFired();

            var target = _item.SphereCastNetworkIdentity(range, TargetMask, _settings.SmallSphereCastRadius);
            if (target != null)
            {
                CmdPerformSwap(target);
            }
            return true;
        }
        else
        {
            return false;
        }
    }
}
