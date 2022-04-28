using System.Collections;
using UnityEngine;
using Mirror;

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