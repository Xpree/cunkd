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

    [SerializeField] GameObject EffectSphere;

    [SerializeField] Transform PointOfFire;
    [SerializeField] GameObject sniper;

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

        if (beam)
        {
            GameObject.Destroy(beam);
            GameObject.Destroy(beam2);
            GameObject.Destroy(beam3);
        }
    }

    void SpawnEffect(Vector3 hitposition)
    {
        if (beam)
        {
            GameObject.Destroy(beam);
            GameObject.Destroy(beam2);
            GameObject.Destroy(beam3);
        }

        beam = Instantiate(EffectSphere, PointOfFire.position, Quaternion.identity);
        beam2 = Instantiate(EffectSphere, PointOfFire.position, Quaternion.identity);
        beam3 = Instantiate(EffectSphere, PointOfFire.position, Quaternion.identity);
        fix = PointOfFire.position;
        HitDetected = hitposition;
        lerpval = 0;
        lerpval2 = 0;
        lerpval3 = 0;
    }

    [Command]
    void cmdFire(Vector3 hitposition)
    {
        rpcFire(hitposition);
    }

    [ClientRpc(includeOwner = false)]
    void rpcFire(Vector3 hitposition)
    {
        SpawnEffect(hitposition);
    }

    // Vector used for calculating trajectory of the beam-effect (purely visual, not actually used for hit-detection)
    Vector3 HitDetected;

    // Objects that the beam-effect is bound to
    GameObject beam;
    GameObject beam2;
    GameObject beam3;

    // Casts a spherecast and checks if a "Swappable" target was hit. Calls the swapping-fuction if so. Also instantiates the beam-effect, since that will play regardless of what is hit.
    public NetworkIdentity DidHitObject()
    {
        // Resets this value used for the Lerping-function.

        var identity = _item.ProjectileHitscanRigidbody<NetworkIdentity>(range, out HitDetected);
        if(identity != null && (identity.GetComponent<Pullable>() || identity.GetComponent<GameClient>()) == false)
        {
            identity = null;
        }

        SpawnEffect(HitDetected);
        cmdFire(HitDetected);

        return identity;
    }

    // Values used for Lerping.
    Vector3 fix;
    float lerpval = 0;
    float lerpval2 = 0;
    float lerpval3 = 0;
    public float speed = 1;

    // Lerps the beam-effect.
    private void Update()
    {
        if (beam)
        {
            beam.transform.position = Vector3.Lerp(fix, HitDetected, lerpval);
            beam2.transform.position = Vector3.Lerp(fix, HitDetected, lerpval2);
            beam3.transform.position = Vector3.Lerp(fix, HitDetected, lerpval3);
            lerpval += Time.deltaTime * (speed / (fix - HitDetected).magnitude) * 70;
            lerpval2 += Time.deltaTime * (speed / (fix - HitDetected).magnitude) * 30;
            lerpval3 += Time.deltaTime * (speed / (fix - HitDetected).magnitude) * 10;

            if (beam.transform.position == HitDetected && beam2.transform.position == HitDetected && beam3.transform.position == HitDetected)
            {
                GameObject.Destroy(beam);
                GameObject.Destroy(beam2);
                GameObject.Destroy(beam3);
            }
        }

    }

    [ClientRpc]
    void PlayTeleport(NetworkIdentity target)
    {
        FMODUnity.RuntimeManager.PlayOneShotAttached("event:/SoundStudents/SFX/Gadgets/Teleporter", target.gameObject);
    }

    // Performs the swap and sets off particle- and sound effects.
    [Command]
    void CmdPerformSwap(NetworkIdentity target, Vector3 targetPosition, Vector3 origin)
    {
        if (target == null || _cooldownTimer.ServerUse(this.cooldown) == false)
        {
            // Client predicted wrong. Dont care!
            return;
        }

        var owner = GetComponent<NetworkItem>()?.Owner;
        if (owner == null)
            return;

        Util.Teleport(target.gameObject, origin);
        Util.Teleport(owner.gameObject, targetPosition);
        animator.SetTrigger("Fire");
        PlayTeleport(target);
        PlayTeleport(owner.GetComponent<NetworkIdentity>());
    }

    // An artificial delay that makes sure that the swapping does not occur immediately after hit-detection. (This gives time to see the beam-effect.)
    System.Collections.IEnumerator DelaySwap(NetworkIdentity target)
    {
        Vector3 position = _item.Owner.transform.position;
        
        if (target == null || target.gameObject.Invulnerabiliy())
            yield break;

        yield return new WaitForSeconds(0.15f);
        ZoomOff();
        CmdPerformSwap(target, HitDetected, position);
    }

    // Upon left-clicking, DidHitObject is called. (Unless the cooldown has not yet reached zero.)
    void IWeapon.PrimaryAttack(bool isPressed)
    {
        if (isPressed)
        {
            if (_cooldownTimer.Use(this.cooldown))
            {
                StartCoroutine(DelaySwap(DidHitObject()));
            }
        }
    }

    // Upon right-clicking, Zoomtoggle is called.
    void IWeapon.SecondaryAttack(bool isPressed)
    {
        if (isPressed)
        {
            FMODUnity.RuntimeManager.PlayOneShotAttached("event:/SoundStudents/SFX/Weapons/Swap rifle scoope sound", this.gameObject);
            ZoomToggle();
        }
    }

    // Toggles the zoom on or off, by calling ToggleZoom. Activates the "scopeCanvas" if zoom turns on.
    void ZoomToggle()
    {
        if (zoomed == false)
        {
            sniper.SetActive(false);
            scopeCanvas.SetActive(true);
            zoomed = true;
        }
        else if (zoomed == true)
        {
            sniper.SetActive(true);
            scopeCanvas.SetActive(false);
            zoomed = false;
        }

        var camera = _item.Owner.GetComponentInChildren<PlayerCameraController>();
        camera.ToggleZoom();
    }

    // Turns off the zoom, and is used when zoom needs to be turned off without right-clicking.
    void ZoomOff()
    {
        sniper.SetActive(true);
        scopeCanvas.SetActive(false);
        zoomed = false;
        if (_item.Owner == null)
        {
            return;
        }

        var camera = _item.Owner.GetComponentInChildren<PlayerCameraController>();
        if (camera == null)
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
