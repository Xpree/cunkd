using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.VFX;

[RequireComponent(typeof(NetworkItem))]
[RequireComponent(typeof(NetworkCooldown))]
public class BlackHoleGun : NetworkBehaviour, IWeapon, IEquipable
{
    [SerializeField] NetworkAnimator animator;

    [SerializeField] GameSettings _settings;
    float Cooldown => _settings.BlackHoleGun.Cooldown;
    float MaxRange => _settings.BlackHoleGun.Range;

    [SerializeField] GameObject blackHole;
    [SerializeField] LayerMask TargetMask = ~0;

    NetworkCooldown _cooldownTimer;

    bool HasTicked;

    void Awake()
    {
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
    void CmdSpawnBlackHole(Vector3 target)
    {
        if (_cooldownTimer.ServerUse(this.Cooldown))
        {
            animator.SetTrigger("Fire");
            HasTicked = false;
            var go = Instantiate(blackHole, target, Quaternion.identity);
            NetworkServer.Spawn(go);
        }
    }

    void IWeapon.PrimaryAttack(bool isPressed)
    {
        if (isPressed)
        {
            if (_cooldownTimer.Use(this.Cooldown))
            {
                //FMODUnity.RuntimeManager.PlayOneShot("event:/SoundStudents/SFX/Weapons/BlackHoleGun");
                //FMODUnity.RuntimeManager.PlayOneShotAttached("event:/SoundStudents/SFX/Weapons/Black Hole", blackHole);
                var aimTransform = Util.GetOwnerAimTransform(GetComponent<NetworkItem>());
                var target = Util.RaycastPointOrMaxDistance(aimTransform, MaxRange, TargetMask);
                CmdSpawnBlackHole(target);
            }
        }
    }

    [ServerCallback]
    void FixedUpdate()
    {
        if (_cooldownTimer.HasCooldown == false && HasTicked == false)
        {
            animator.SetTrigger("Ready");
            HasTicked = true;
        }
    }

    void IWeapon.SecondaryAttack(bool isPressed)
    {

    }

    float? IWeapon.ChargeProgress => null;

    #region IEquipable
    bool holstered;
    bool IEquipable.IsHolstered => holstered;

    System.Collections.IEnumerator TestAnimation()
    {
        var start = NetworkTimer.Now;

        for (; ; )
        {
            var t = start.Elapsed * 5;
            if (t > 0.99)
            {
                break;
            }

            transform.localScale = Vector3.one * (float)(1.0 - t);
            yield return null;
        }
        transform.localScale = Vector3.zero;
        holstered = true;
    }


    void IEquipable.OnHolstered()
    {
        StartCoroutine(TestAnimation());
    }

    void IEquipable.OnUnholstered()
    {
        // TODO Animation then set holstered
        holstered = false;
        transform.localScale = Vector3.one;
    }

    void IEquipable.OnPickedUp(bool startHolstered)
    {
        holstered = startHolstered;

        if (holstered)
            transform.localScale = Vector3.zero;
        else
            transform.localScale = Vector3.one;
    }

    void IEquipable.OnDropped()
    {
        this.transform.parent = null;
        if (holstered)
        {
            holstered = false;
            transform.localScale = Vector3.one;
        }
    }

    #endregion
}