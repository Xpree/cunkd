using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkItem))]
[RequireComponent(typeof(NetworkCooldown))]
public class BlackHoleGun : NetworkBehaviour
{    
    [SerializeField] GameSettings _settings;
    public float Cooldown => _settings.BlackHoleGun.Cooldown;
    public float MaxRange => _settings.BlackHoleGun.Range;

    [SerializeField] GameObject blackHole;
    [SerializeField] public LayerMask TargetMask = ~0;

    NetworkCooldown _cooldownTimer;
    NetworkItem _item;
    void Awake()
    {
        _item = GetComponent<NetworkItem>();
        _item.ItemType = ItemType.Weapon;

        _cooldownTimer = GetComponent<NetworkCooldown>();
        _cooldownTimer.CooldownDuration = Cooldown;
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
        }
    }

    public bool Shoot()
    {
        if(_cooldownTimer.Use())
        {
            _item.OnPrimaryAttackFired();

            var aim = Util.GetOwnerAimTransform(GetComponent<NetworkItem>());
            var target = Util.RaycastPointOrMaxDistance(aim, MaxRange, TargetMask);
            CmdSpawnBlackHole(target);
            return true;
        }
        else
        {
            return false;
        }
    }
}
