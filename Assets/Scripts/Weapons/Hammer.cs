using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.VFX;

[RequireComponent(typeof(NetworkItem))]
[RequireComponent(typeof(NetworkCooldown))]
public class Hammer : NetworkBehaviour, IWeapon, IEquipable
{
    [SerializeField] NetworkAnimator Netanimator;
    [SerializeField] Animator animator;

    [SerializeField] GameObject Head;

    [SerializeField] GameSettings _settings;
    float Cooldown => _settings.Hammer.Cooldown;
    float Radius => _settings.Hammer.Radius;
    float Force => _settings.Hammer.Force;
    [SerializeField] LayerMask TargetMask = ~0;

    NetworkCooldown _cooldownTimer;

    float offTime = 1.3f;

    bool Swung;
    bool HasTicked;

    void Awake()
    {
        animator = gameObject.GetComponent<Animator>();
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

    //[Command]
    //void CmdSpawnBlackHole(Vector3 target)
    //{
    //    if (_cooldownTimer.ServerUse(this.Cooldown))
    //    {
    //        animator.SetTrigger("Fire");
    //        HasTicked = false;
    //        var go = Instantiate(blackHole, target, Quaternion.identity);
    //        NetworkServer.Spawn(go);
    //    }
    //}

    [Command]
    void Smash()
    {
        var owner = GetComponent<NetworkItem>()?.Owner;
        if (owner == null)
            return;

        HasTicked = false;
        Collider[] colliders = Physics.OverlapSphere(Head.transform.position, Radius);
        foreach (Collider nearby in colliders)
        {
            Rigidbody rb = nearby.GetComponent<Rigidbody>();
            if (rb != null)
            {
                if(rb != owner.GetComponent<Rigidbody>())
                {
                    FMODUnity.RuntimeManager.PlayOneShot("event:/SoundStudents/SFX/Gadgets/Hammer/Hammer Hit");
                    rb.AddExplosionForce(Force, Head.transform.position, Radius);
                }
                else
                {
                    FMODUnity.RuntimeManager.PlayOneShot("event:/SoundStudents/SFX/Gadgets/Hammer/Hammer Miss");
                }
            }
            else
            {
                FMODUnity.RuntimeManager.PlayOneShot("event:/SoundStudents/SFX/Gadgets/Hammer/Hammer Miss");
            }
        }
        
        
    }




    void IWeapon.PrimaryAttack(bool isPressed)
    {
        if (isPressed)
        {
            if (_cooldownTimer.Use(this.Cooldown))
            {
                animator.SetTrigger("Swing");
                offTime = 1.3f;
            }
            else
            {

            }
        }
        //else
        //{
        //    animator.SetBool("swingNew", false);
        //}
    }

    void IWeapon.SecondaryAttack(bool isPressed)
    {

    }

    [ServerCallback]
    void FixedUpdate()
    {
        if (offTime >= 0)
            offTime = offTime - 1 * Time.deltaTime;
        if (_cooldownTimer.HasCooldown == false && HasTicked == false)
        {
            HasTicked = true;
        }
        if(offTime <= 0)
        {
            animator.SetBool("swingNew", false);
        }
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