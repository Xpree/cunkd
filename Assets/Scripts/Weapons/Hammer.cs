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

    [SerializeField] GameObject Head;

    [SerializeField] GameSettings _settings;
    float Cooldown => _settings.Hammer.Cooldown;
    float Radius => _settings.Hammer.Radius;
    float Force => _settings.Hammer.Force;

    NetworkCooldown _cooldownTimer;

    [SerializeField] CameraShakeSource _cameraShake;


    bool Swung;
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

    
    void Smash()
    {
        var owner = GetComponent<NetworkItem>()?.Owner;
        if (owner == null)
            return;

        HasTicked = false;
        Collider[] colliders = Physics.OverlapSphere(Head.transform.position, Radius, ~0, QueryTriggerInteraction.Ignore);
        if(colliders.Length > 0)
        {
            _cameraShake.OneShotShake(NetworkTimer.Now);
        }
        foreach (Collider nearby in colliders)
        {
            Rigidbody rb = nearby.GetComponent<Rigidbody>();
            if (rb != null)
            {
                if (rb != owner.GetComponent<Rigidbody>() && rb.CompareTag("Player"))
                {
                    //Debug.Log("PlayerHit");                    
                    FMODUnity.RuntimeManager.PlayOneShotAttached("event:/SoundStudents/SFX/Gadgets/Hammer/Hammer Hit", this.gameObject);
                    rb.AddExplosionForce(Force * 2, Head.transform.position, Radius);
                }
                if(rb != owner.GetComponent<Rigidbody>())
                {
                    //Debug.Log("ObjectHit");                    
                    rb.AddExplosionForce(Force, Head.transform.position, Radius);
                }
                else
                {
                    //Debug.Log("Miss");                    
                    FMODUnity.RuntimeManager.PlayOneShotAttached("event:/SoundStudents/SFX/Gadgets/Hammer/Hammer Miss", this.gameObject);
                }
            }            
        }
    }

    void IWeapon.PrimaryAttack(bool isPressed)
    {
        if (isPressed)
        {
            if (_cooldownTimer.Use(this.Cooldown))
            {
                Netanimator.SetTrigger("Swing");
                //Swung = true;
            }
        }
    }

    void IWeapon.SecondaryAttack(bool isPressed)
    {

    }

    [ServerCallback]
    void FixedUpdate()
    {
        if (_cooldownTimer.HasCooldown == false && HasTicked == false)
        {
            HasTicked = true;
        }
        //if(Swung == true)
        //{
        //    Swung = false;
        //    Smash();
        //}
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