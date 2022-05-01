using UnityEngine;
using Mirror;
using Unity.VisualScripting;

[RequireComponent(typeof(NetworkItem))]
[RequireComponent(typeof(NetworkCooldown))]
public class BlackHoleGun : NetworkBehaviour
{
    [SerializeField] GameObject blackHolePrefab;

    NetworkCooldown cooldownTimer;
    NetworkItem item;


    void Awake()
    {
        item = GetComponent<NetworkItem>();
        item.ItemType = ItemType.Weapon;

        var settings = GameServer.Instance.Settings.BlackHoleGun;
        cooldownTimer = GetComponent<NetworkCooldown>();
        cooldownTimer.CooldownDuration = settings.Cooldown;
    }

    [Command]
    public void CmdSpawnBlackHole(Vector3 target)
    {
        if (cooldownTimer.ServerUse())
        {
            NetworkEventBus.TriggerExcludeOwner(nameof(EventPrimaryAttackFired), this.netIdentity);
            var go = Instantiate(blackHolePrefab, target, Quaternion.identity);
            NetworkServer.Spawn(go);
        }
    }

    public bool Shoot()
    {
        if (cooldownTimer.Use())
        {
            // Note: This is always successful so CmdSpawnBlackHole can handle triggering the clients
            EventBus.Trigger(nameof(EventPrimaryAttackFired), this.gameObject);

            var settings = GameServer.Instance.Settings.BlackHoleGun;
            var target = item.RaycastPointOrMaxDistance(settings.Range, settings.TargetMask);
            CmdSpawnBlackHole(target);

            return true;
        }
        else
        {
            return false;
        }
    }
}
