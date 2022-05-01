using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkItem))]
[RequireComponent(typeof(NetworkCooldown))]
public class IceGadget : NetworkBehaviour
{
    public GameObject IceGadgetTrap;
    [SerializeField] LayerMask TargetMask = ~0;
    NetworkItem item;

    // Mostly for displaying 1/1 in UI
    NetworkCooldown cooldown;

    void Awake()
    {
        item = GetComponent<NetworkItem>();
        item.ItemType = ItemType.Gadget;

        var settings = GameServer.Instance.Settings.IceGadget;
        cooldown = GetComponent<NetworkCooldown>();
        cooldown.MaxCharges = settings.Charges;
        cooldown.CooldownDuration = settings.Cooldown;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        cooldown.SetCharges(1);
    }

    [Command]
    void SpawnIceGadget(Vector3 target)
    {
        if(cooldown.ServerUse())
        {
            var go = Instantiate(IceGadgetTrap, target, Quaternion.identity);
            NetworkServer.Spawn(go);
            if(cooldown.Charges == 0)
                NetworkServer.Destroy(this.gameObject);
        }
    }

    public bool Use()
    {
        if(cooldown.Use())
        {
            var settings = GameServer.Instance.Settings.IceGadget;

            // TODO: Throwing logic?
            var point = item.ProjectileHitscanPoint(settings.MaxRange);
            SpawnIceGadget(point);
            return true;
        }
        else
        {
            return false;
        }
    }

}
