using UnityEngine;
using Mirror;
using Unity.VisualScripting;

[RequireComponent(typeof(NetworkItem))]
[RequireComponent(typeof(NetworkCooldown))]
public class SwapSniper : NetworkBehaviour
{
    NetworkCooldown cooldown;
    NetworkItem item;

    void Awake()
    {
        item = GetComponent<NetworkItem>();
        item.ItemType = ItemType.Weapon;

        var settings = GameServer.Instance.Settings.SwapSniper;
        cooldown = GetComponent<NetworkCooldown>();
        cooldown.CooldownDuration = settings.Cooldown;
    }

    [Command]
    void CmdPerformSwap(NetworkIdentity target)
    {
        var settings = GameServer.Instance.Settings.SwapSniper;

        if (target == null || cooldown.ServerUse(settings.Cooldown) == false)
        {
            // Client predicted wrong. Dont care!
            return;
        }

        var owner = item.Owner;
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
        var settings = GameServer.Instance.Settings.SwapSniper;
        if (this.netIdentity.HasControl() && cooldown.Use(settings.Cooldown))
        {
            EventBus.Trigger(nameof(EventPrimaryAttackFired), this.gameObject);
            CmdTriggerPrimaryAttackFired();

            var target = item.ProjectileHitscanIdentity(settings.Range);
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
