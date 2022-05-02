using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using UnityEngine.VFX;
using Unity.VisualScripting;

[RequireComponent(typeof(NetworkItem))]
public class GravityGun : NetworkBehaviour
{
    [SerializeField] NetworkAnimator animator;


    [SerializeField] GameSettings _settings;

    [SerializeField] Transform AnchorPoint;
    [SerializeField] LayerMask TargetMask = ~0;

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

    bool _pulling;
    GameObject _localPullObject;

    NetworkItem item;

   
    private void Awake()
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

    private void OnDestroy()
    {
        StopPulling();
    }

    private void OnEnable()
    {
        EventBus.Register(new EventHook(nameof(EventItemDeactivated), item.gameObject), new System.Action<EmptyEventArgs>(OnItemDeactivated));
    }

    private void OnDisable()
    {
        EventBus.Unregister(new EventHook(nameof(EventItemDeactivated), item.gameObject), new System.Action<EmptyEventArgs>(OnItemDeactivated));
    }

    void OnItemDeactivated(EmptyEventArgs args)
    {
        StopPulling();
    }

    bool HasPullObject()
    {
        return _localPullObject != null && _localPullObject.activeSelf;
    }

    void StopPulling()
    {
        if(HasPullObject())
        {
            _localPullObject.GetComponent<Pullable>().StopPulling();
        }
        _localPullObject = null;
        _pulling = false;
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        StopPulling();
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        StopPulling();
    }


    void DoPullObject(NetworkIdentity identity, NetworkTimer grabEnd)
    {
        var pullable = identity.GetComponent<Pullable>();
        var offset = pullable.pullingCollider.bounds.extents.magnitude;
        pullable.StartPulling(AnchorPoint.gameObject, offset, grabEnd);
        pullable.body.AddTorque(Random.Range(-GrabTorque, GrabTorque), Random.Range(-GrabTorque, GrabTorque), Random.Range(-GrabTorque, GrabTorque));
        _localPullObject = identity.gameObject;
        _pulling = true;
    }

    [ClientRpc]
    void RpcPullObject(NetworkIdentity identity, NetworkTimer grabEnd)
    {
        DoPullObject(identity, grabEnd);
    }

    [Command]
    void CmdPull(NetworkIdentity identity)
    {       
        if (identity == null)
            return;

        if(HasPullObject())
        {
            if(_pulling)
            {
                NetworkEventBus.TriggerAll(nameof(EventPrimaryAttackEnd), this.netIdentity);
            }
            StopPulling();
            RpcStopPulling();
        }
        
        NetworkEventBus.TriggerAll(nameof(EventPrimaryAttackBegin), this.netIdentity);
        var grabEnd = NetworkTimer.FromNow(GrabTime);
        RpcPullObject(identity, grabEnd);
        if (this.isServerOnly)
            DoPullObject(identity, grabEnd);
    }

    [ClientRpc]
    void RpcStopPulling()
    {
        StopPulling();
    }

    [Command]
    void CmdPush(Vector3 position, Vector3 direction)
    {
        if (_localPullObject == null)
            return;

        var body = _localPullObject.GetComponent<Rigidbody>();

        var isKinematic = body.isKinematic;
        StopPulling();
        RpcStopPulling();
        if(isKinematic)
        {
            NetworkEventBus.TriggerAll(nameof(EventPrimaryAttackFired), this.netIdentity);
            // TODO: Add NetworkTransform/NetworkRigidbody force update?
            body.position = position;
            body.velocity = direction.normalized * MaxPushForce;
        }
    }

    public bool StartAttacking()
    {
        if(this.netIdentity.HasControl() && HasPullObject() == false)
        {
            if (Physics.SphereCast(item.AimRay, 0.5f, out RaycastHit hitResult, MaxGrabRange, TargetMask))
            {
                var pullable = hitResult.collider.GetComponent<Pullable>();
                
                if(pullable != null && pullable.IsBeingPulled == false)
                {
                    CmdPull(pullable.netIdentity);
                    return true;
                }
            }
        }
        return false;
    }

    public bool StopAttacking()
    {
        if (this.netIdentity.HasControl() && HasPullObject())
        {
            Vector3 aimPoint = item.ProjectileHitscanPoint(100.0f);
            Vector3 pushOrigin = _localPullObject.transform.position;
            CmdPush(pushOrigin, (aimPoint - pushOrigin).normalized);
            return true;
        }
        else
        {
            return false;
        }
    }

    private void FixedUpdate()
    {
        if(_pulling && HasPullObject() && _localPullObject.GetComponent<Rigidbody>().isKinematic)
        {
            _pulling = false;
            NetworkEventBus.TriggerAll(nameof(EventPrimaryAttackEnd), this.netIdentity);
        }
    }
}

