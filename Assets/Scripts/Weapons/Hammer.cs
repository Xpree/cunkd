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

    NetworkItem item;
    bool busy;

    void Awake()
    {
        _cooldownTimer = GetComponent<NetworkCooldown>();
        _cooldownTimer.CooldownDuration = Cooldown;

        item = GetComponent<NetworkItem>();
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
        if (busy || holstered)
            return;
        
        var owner = GetComponent<NetworkItem>()?.Owner;
        if (owner == null)
            return;

        HasTicked = false;
        Collider[] colliders = Physics.OverlapSphere(Head.transform.position, Radius, ~0, QueryTriggerInteraction.Ignore);
        if(colliders.Length > 0)
        {
            FMODUnity.RuntimeManager.PlayOneShotAttached("event:/SoundStudents/SFX/Gadgets/Hammer/Hammer Hit", this.Head);
            _cameraShake.OneShotShake(NetworkTimer.Now);
        }
        else
        {
            FMODUnity.RuntimeManager.PlayOneShotAttached("event:/SoundStudents/SFX/Gadgets/Hammer/Hammer Miss", this.gameObject);
        }
        
        foreach (Collider nearby in colliders)
        {
            Rigidbody rb = nearby.GetComponent<Rigidbody>();
            if (rb != null && rb != owner.GetComponent<Rigidbody>())
            {
                if(rb.CompareTag("Player"))
                {
                    if(rb.gameObject.Invulnerabiliy() == false)
                    {
                        rb.AddExplosionForce(Force * 2, Head.transform.position, Radius);
                        rb.GetComponent<PlayerMovement>().NoFriction();
                        FMODUnity.RuntimeManager.PlayOneShotAttached("event:/SoundStudents/SFX/Environment/Cat sound when dying", rb.gameObject);
                    }
                }
                else
                {
                    rb.AddExplosionForce(Force, Head.transform.position, Radius);
                }                
            }            
        }
    }

    void IWeapon.PrimaryAttack(bool isPressed)
    {
        if (isPressed)
        {
            float cooldown = this.Cooldown;
            if(item.IsOwnerCunkd)
            {
                cooldown *= 0.5f;
            }
            
            if (_cooldownTimer.Use(cooldown))
            {
                //speeds up animation
                // Netanimator.animator.speed = 1f;
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

    void IEquipable.OnHolstered()
    {
        holstered = true;
        transform.localScale = Vector3.zero;
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