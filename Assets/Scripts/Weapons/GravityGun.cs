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
            AudioHelper.PlayOneShotWithParameters("event:/SoundStudents/SFX/Weapons/Gravity Gun", this.transform.position, ("Grab Object", 1f), ("Object recived start loading", 1f), ("Shot away object", 1f));
            var target = targetObject;
            if (target == null || target.GetComponent<Pullable>().IsFixed == false)
                target = item.ProjectileHitscanIdentity(MaxRange)?.gameObject;
            
            if(target != null && target.GetComponent<GameClient>() == null)
            {
                var progress = GetChargeProgress();
                justStop();
                CmdPush(target, this.transform.forward, progress);
            }
        }
        
    }

    void IWeapon.SecondaryAttack(bool isPressed)
    {
        justStop();
        if (isPressed)
        {
            var target = item.ProjectileHitscanOwner<Pullable>(MaxGrabRange);
            if(target == null)
            {
                return;
            }
            //AudioHelper.PlayOneShotAttachedWithParameters("event:/SoundStudents/SFX/Weapons/Gravity Gun", this.AnchorPoint.gameObject, ("Grab Object", 1f), ("Object recived start loading", 1f), ("Shot away object", 0f));
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
        animator.SetTrigger("Fire");
        justStop();
        Vector3 torque = new Vector3(Random.Range(-GrabTorque, GrabTorque), Random.Range(-GrabTorque, GrabTorque), Random.Range(-GrabTorque, GrabTorque));
        var body = target.GetComponent<Rigidbody>();
        float Force = Mathf.Lerp(MinPushForce, MaxPushForce, Mathf.Clamp01(progress));
        body.AddForce(aimDirection * Force, PushForceMode);
        body.AddTorque(torque);
    }

    void StartPulling(Pullable target, NetworkTimer time, Vector3 torque)
    {
        if (target == null || target.IsBeingPulled)
        {
            return;
        }
        target.GetComponent<Rigidbody>().AddTorque(torque);
        target.StartPulling(AnchorPoint.gameObject, time);
        targetObject = target.gameObject;
    }

    void StopPulling()
    {
        if (targetObject == null)
        {
            return;
        }
        targetObject.GetComponent<Pullable>().StopPulling();
        targetObject = null;
    }

    [Command]
    void CmdPull(Pullable target)
    {
        animator.SetTrigger("Attract");
        Vector3 torque = new Vector3(Random.Range(-GrabTorque, GrabTorque), Random.Range(-GrabTorque, GrabTorque), Random.Range(-GrabTorque, GrabTorque));
        var time = NetworkTimer.FromNow(GrabTime);
        if (this.isServerOnly)
        {
            StartPulling(target, time, torque);
        }
        RpcPull(target, time, torque);
    } 

    [ClientRpc]
    void RpcPull(Pullable target, NetworkTimer endTime, Vector3 torque)
    {
        StartPulling(target, endTime, torque);
    }

    [Command]
    void CmdStopAttract()
    {
        if (targetObject != null)
        {
            animator.SetTrigger("StopAttract");
        }
    }

    [Command]
    void CmdStopPulling()
    {
        if (isServerOnly)
        {
            StopPulling();
        }
        RpcStopPulling();
    }

    [ClientRpc(includeOwner = false)]
    void RpcStopPulling()
    {
        StopPulling();
    }

    void justStop()
    {
        CmdStopAttract();
        StopPulling();
        CmdStopPulling();
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

