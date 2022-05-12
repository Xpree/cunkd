using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.VFX;

[RequireComponent(typeof(NetworkItem))]
[RequireComponent(typeof(NetworkCooldown))]
public class SwapSniper : NetworkBehaviour, IWeapon, IEquipable
{
    [SerializeField] NetworkAnimator animator;
    public GameObject scopeCanvas;
    [SerializeField] GameSettings _settings;
    float cooldown => _settings.SwapSniper.Cooldown;
    float range => _settings.SwapSniper.Range;

    [SerializeField] LayerMask TargetMask = ~0;

    NetworkCooldown _cooldownTimer;

    NetworkItem _item;
    public bool zoomed;
    void Awake()
    {
        _item = GetComponent<NetworkItem>();
        _cooldownTimer = GetComponent<NetworkCooldown>();
        _cooldownTimer.CooldownDuration = cooldown;
    }

    private void Start()
    {
        zoomed = false;
        if (_settings == null)
        {
            Debug.LogError("Missing GameSettings reference on " + name);
        }
    }

    void OnDisable()
    {
        ZoomOff();
    }
    
    public NetworkIdentity DidHitObject()
    {
        var aimTransform = Util.GetOwnerAimTransform(GetComponent<NetworkItem>());
        if (Physics.SphereCast(aimTransform.position, 0.5f, aimTransform.forward, out RaycastHit hitResult, range, TargetMask))
        {            
            return hitResult.rigidbody?.GetComponent<NetworkIdentity>();
        }
        else
        {
            return null;
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

        var owner = GetComponent<NetworkItem>()?.Owner;
        if (owner == null)
            return;

        Vector3 Swapper = owner.transform.position;
        Vector3 Swappee = target.transform.position;

        Util.Teleport(target.gameObject, Swapper);
        Util.Teleport(owner.gameObject, Swappee);
        animator.SetTrigger("Fire");
        FMODUnity.RuntimeManager.PlayOneShotAttached("event:/SoundStudents/SFX/Gadgets/Teleporter", target.gameObject);
        FMODUnity.RuntimeManager.PlayOneShotAttached("event:/SoundStudents/SFX/Gadgets/Teleporter", owner.gameObject);
    }

    System.Collections.IEnumerator DelaySwap(NetworkIdentity target)
    {
        if (target == null)
            yield break;
        
        yield return new WaitForSeconds(0.05f);
        ZoomOff();
        CmdPerformSwap(target);
    }

    void IWeapon.PrimaryAttack(bool isPressed)
    {
        if(isPressed)
        {
            if(_cooldownTimer.Use(this.cooldown))
            {
                StartCoroutine(DelaySwap(DidHitObject()));
            }
        }
    }
    void IWeapon.SecondaryAttack(bool isPressed)
    {
        if (isPressed)
        {
            ZoomToggle();
        }
    }
    void ZoomToggle()
    {
        if (zoomed == false)
        {
            scopeCanvas.SetActive(true);
            zoomed = true;
        }
        else if (zoomed == true)
        {
            scopeCanvas.SetActive(false);
            zoomed = false;
        }
        
        var camera = _item.Owner.GetComponentInChildren<PlayerCameraController>();
        camera.ToggleZoom();
    }

    void ZoomOff()
    {
        scopeCanvas.SetActive(false);
        zoomed = false;
        if(_item.Owner == null)
        {
            return;
        }

        var camera = _item.Owner.GetComponentInChildren<PlayerCameraController>();
        if(camera == null)
        {
            return;
        }
        camera.ZoomOff();
    }

    

    float? IWeapon.ChargeProgress => null;


    #region IEquipable

    bool holstered;
    bool IEquipable.IsHolstered => holstered;

    void IEquipable.OnHolstered()
    {
        ZoomOff();
        // TODO Animation then set holstered
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
        ZoomOff();
        this.transform.parent = null;
        if (holstered)
        {
            holstered = false;
            transform.localScale = Vector3.one;
        }
    }
    #endregion
}
