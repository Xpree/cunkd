using UnityEngine;
using Mirror;

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


    public bool Shoot()
    {
        if (_cooldownTimer.Use(this.cooldown))
        {
            _item.OnPrimaryAttackFired();

            var aimTransform = Util.GetOwnerAimTransform(GetComponent<NetworkItem>());
            if (Physics.SphereCast(aimTransform.position, 0.25f, aimTransform.forward, out RaycastHit hitResult, range, TargetMask))
            {
                var target = hitResult.rigidbody?.GetComponent<NetworkIdentity>();
                if(target != null)
                {
                    CmdPerformSwap(target);
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }
}
