using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using UnityEngine.VFX;

[RequireComponent(typeof(NetworkItem))]
public class GravityGun : NetworkBehaviour, IWeapon, IEquipable
{
    [SerializeField] NetworkAnimator animator;


    [SerializeField] GameSettings _settings;

    [SerializeField] Transform AnchorPoint;
    [SerializeField] LayerMask TargetMask = ~0;


    [SerializeField] Collider PlayerCollider;
    [SerializeField] ForceMode PushForceMode = ForceMode.Impulse;
    //grab
    float MaxGrabRange => _settings.GravityGun.MaxGrabRange;
    float GrabTime => _settings.GravityGun.GrabTime;
    float GrabTorque => _settings.GravityGun.GrabTorque;

    //push
    float MinPushForce => _settings.GravityGun.MinPushForce;
    float MaxPushForce => _settings.GravityGun.MaxPushForce;
    float ChargeRate => _settings.GravityGun.ChargeRate;
    float MaxRange => _settings.GravityGun.MaxRange;

    NetworkItem item;
    NetworkTimer chargeBegan;
    GameObject targetObject;


    void Awake()
    {
        item = GetComponent<NetworkItem>();
    }

    private void Start()
    {
        if (_settings == null)
        {
            Debug.LogError("Missing GameSettings reference on " + name);
        }
    }

    void IWeapon.PrimaryAttack(bool isPressed)
    {
        charging = isPressed;
        if (isPressed)
        {
            chargeBegan = NetworkTimer.Now;
            return;
        }
        else
        {
            CmdStopPulling();
            var progress = GetChargeProgress();

            var pushedObject = item.ProjectileHitscanIdentity(MaxRange);

            if(pushedObject == null)
            {
                return;
            }
            CmdPush(pushedObject.gameObject, this.transform.forward, progress);
        }
        
    }

    void IWeapon.SecondaryAttack(bool isPressed)
    {
        CmdStopPulling();
        if (isPressed)
        {
            var target = item.ProjectileHitscanComponent<Pullable>(MaxGrabRange);
            if(target == null)
            {
                return;
            }
            CmdPull(target);
        }
    }

    float GetChargeProgress()
    {
        return Mathf.Clamp01((float)chargeBegan.Elapsed);
    }

    [Command]
    void CmdPush(GameObject target, Vector3 aimDirection, float progress)
    {
        float Force = Mathf.Lerp(MinPushForce, MaxPushForce, Mathf.Clamp01(progress));
        target.GetComponent<Rigidbody>().AddForce(aimDirection * Force, PushForceMode);
    }

    void StartPulling(Pullable target, NetworkTimer time)
    {
        if (target == null || target.IsBeingPulled)
        {
            return;
        }
        target.StartPulling(AnchorPoint.gameObject, 0, time);
        targetObject = target.gameObject;
    }

    void StopPulling()
    {
        if (targetObject == null)
        {
            return;
        }
        targetObject.GetComponent<Pullable>().StopPulling();
    }

    [Command]
    void CmdPull(Pullable target)
    {
        var time = NetworkTimer.FromNow(GrabTime);
        if (this.isServerOnly)
        {
            StartPulling(target, time);
        }
        RpcPull(target, time);
    } 

    [ClientRpc]
    void RpcPull(Pullable target, NetworkTimer endTime)
    {
        StartPulling(target, endTime);
    }

    [Command]
    void CmdStopPulling()
    {
        StopPulling();
        RpcStopPulling();
    }

    [ClientRpc]
    void RpcStopPulling()
    {
        StopPulling();
    }
    
    void OnDisable()
    {
        StopPulling();
    }



    bool charging;
    float? IWeapon.ChargeProgress => charging ? GetChargeProgress() : null;

    #region IEquipable
    bool holstered = false;
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
        StopPulling();
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

        PlayerCollider = GetComponent<NetworkItem>().Owner.GetComponent<Collider>();
    }


    void IEquipable.OnDropped()
    {
        StopPulling();
        this.transform.parent = null;
        if (holstered)
        {
            holstered = false;
            transform.localScale = Vector3.one;
        }

        PlayerCollider = null;
    }
    #endregion
}

