using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using UnityEngine.VFX;

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
    bool _push = false;
    Vector3 _pushOrigin = Vector3.zero;
    Vector3 _pushDirection = Vector3.zero;
    GameObject _localAttract;

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


    public Vector3 GetAnchorPosition(float offset)
    {
        return AnchorPoint.position + AnchorPoint.forward * offset;
    }

    bool HasLocalPullObject()
    {
        if (_localAttract == null || _localAttract.activeSelf == false)
        {
            _pulling = false;
            _localAttract = null;
            return false;
        }
        return true;
    }

    // Attempts to simulate the pulling locally on each client
    System.Collections.IEnumerator PullObject(NetworkIdentity identity, NetworkTimer grabStart)
    {
        // Stop pulling any exisiting object
        _pulling = false;
        while(_localAttract != null)
        {
            yield return new WaitForFixedUpdate();
        }

        _push = false;
        _pulling = true;
        _localAttract = identity.gameObject;

        Util.SetPhysicsSynchronized(identity, false);

        var body = identity.GetComponent<Rigidbody>();
        var grabTargetCollider = body.GetComponent<Collider>();
        var attractOffset = grabTargetCollider.bounds.extents.magnitude;
        body.AddTorque(Random.Range(-GrabTorque, GrabTorque), Random.Range(-GrabTorque, GrabTorque), Random.Range(-GrabTorque, GrabTorque));
        for (; ; )
        {
            if(!HasLocalPullObject())
            {
                yield break;
            }

            if (!_pulling)
            {
                break;
            }

            var timeToAnchor = (float)(GrabTime - grabStart.Elapsed);
            if (timeToAnchor <= 0)
                break;

            body.velocity = (GetAnchorPosition(attractOffset) - body.position) / timeToAnchor;
            yield return new WaitForFixedUpdate();
        }

        if(NetworkServer.active)
            NetworkEventBus.TriggerAll(nameof(EventPrimaryAttackEnd), this.netIdentity);

        if (_pulling)
        {
            grabTargetCollider.enabled = false;
            //body.transform.parent = this.AnchorPoint;
            body.isKinematic = true;

            //animator.SetTrigger("Hold");
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            while (_pulling)
            {
                body.position = GetAnchorPosition(attractOffset);
                yield return null;
                if (!HasLocalPullObject())
                {
                    yield break;
                }
            }

            body.isKinematic = false;
            //body.transform.parent = null;
            grabTargetCollider.enabled = true;

            if (_push)
            {
                NetworkEventBus.TriggerAll(nameof(EventPrimaryAttackFired), this.netIdentity);
                body.position = _pushOrigin;
                body.AddForce(_pushDirection * this.MaxPushForce, this.PushForceMode);
            }
        }

        Util.SetPhysicsSynchronized(identity, true);
        _localAttract = null;
    }

    void DoPullObject(NetworkIdentity identity, NetworkTimer grabStart)
    {
        StartCoroutine(PullObject(identity, grabStart));
    }

    [ClientRpc]
    void RpcPullObject(NetworkIdentity identity, NetworkTimer grabStart)
    {
        DoPullObject(identity, grabStart);
    }

    [Command]
    void CmdPull(NetworkIdentity identity)
    {       
        if (identity == null)
            return;

        NetworkEventBus.TriggerAll(nameof(EventPrimaryAttackBegin), this.netIdentity);
        var grabStart = NetworkTimer.Now;
        RpcPullObject(identity, grabStart);
        if (this.isServerOnly)
            DoPullObject(identity, grabStart);
    }

    [ClientRpc]
    void RpcStopPulling()
    {
        _pulling = false;
    }

    [Command]
    void CmdPush(Vector3 position, Vector3 direction)
    {
        if (_localAttract == null || !_pulling)
            return;

        _pushOrigin = position;
        _pushDirection = direction.normalized;
        _push = true;
        _pulling = false;
        
        RpcStopPulling();
    }

    public bool StartAttacking()
    {
        if(this.netIdentity.HasControl() && HasLocalPullObject() == false)
        {
            if (Physics.SphereCast(item.AimRay, 0.5f, out RaycastHit hitResult, MaxGrabRange, TargetMask))
            {
                var networkIdentity = hitResult.collider.GetComponent<NetworkIdentity>();
                CmdPull(networkIdentity);
                return true;
            }
        }
        return false;
    }

    public bool StopAttacking()
    {
        if (this.netIdentity.HasControl() && HasLocalPullObject())
        {
            Vector3 aimPoint = item.ProjectileHitscanPoint(100.0f);
            Vector3 pushOrigin = _localAttract.transform.position;
            CmdPush(pushOrigin, (aimPoint - pushOrigin).normalized);
            return true;
        }
        else
        {
            return false;
        }
    }
}

