using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using UnityEngine.VFX;

[RequireComponent(typeof(NetworkItem))]
public class GravityGun : NetworkBehaviour, IWeapon, IEquipable
{
    [SerializeField] GameObject attractVFX;
    [SerializeField] GameObject holdVFX;
    [SerializeField] GameObject pushVFX;


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


    bool _charging;
    NetworkTimer _chargeBegan;

    bool _pulling;
    NetworkIdentity _localAttract;


    private void Start()
    {
        if (_settings == null)
        {
            Debug.LogError("Missing GameSettings reference on " + name);
        }
        attractVFX.GetComponent<VisualEffect>().Stop();
        holdVFX.GetComponent<VisualEffect>().enabled = false;
        pushVFX.GetComponent<VisualEffect>().Stop();
    }

    public Vector3 GetAnchorPosition(float offset)
    {
        return AnchorPoint.position + AnchorPoint.forward * offset;
    }

    System.Collections.IEnumerator PullObject(Rigidbody body)
    {
        var grabTargetCollider = body.GetComponent<Collider>();
        var attractOffset = grabTargetCollider.bounds.extents.magnitude;
        body.AddTorque(Random.Range(-GrabTorque, GrabTorque), Random.Range(-GrabTorque, GrabTorque), Random.Range(-GrabTorque, GrabTorque));
        var grabStart = NetworkTimer.Now;
        for(; ;)
        {
            if (!_pulling)
            {
                yield break;
            }

            var timeToAnchor = (float)(GrabTime - grabStart.Elapsed);
            if(timeToAnchor <= 0)
                break;

            body.velocity = (GetAnchorPosition(attractOffset) - body.position) / timeToAnchor;
            yield return new WaitForFixedUpdate();
        }

        grabTargetCollider.enabled = false;
        body.isKinematic = true;
        holdVFX.GetComponent<VisualEffect>().enabled = true;
        attractVFX.GetComponent<VisualEffect>().Stop();
        body.velocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        while (_pulling)
        {
            body.position = GetAnchorPosition(attractOffset);
            yield return new WaitForFixedUpdate();
        }
    }


    [TargetRpc]
    void TargetRpcBeginAttract(NetworkIdentity body)
    {
        if (_pulling)
        {
            Util.SetClientPhysicsAuthority(body, true);
            _localAttract = body;
            StartCoroutine(PullObject(body.GetComponent<Rigidbody>()));
            attractVFX.GetComponent<VisualEffect>().Play();
            
        }
        else
        {
            CmdStopAttract(body.GetComponent<NetworkIdentity>(), body.transform.position, AnchorPoint.transform.forward, 0);
        }
    }

    [Command]
    void CmdAttract(NetworkIdentity identity)
    {
        if (identity == null)
            return;

        if(_localAttract != null)
        {
            GameServer.PurgeOwnedObjects(this.connectionToClient);
            if(this.connectionToClient != null)
            {
                Debug.LogError("Client tried to attract multiple objects.");
                this.connectionToClient.Disconnect();
            }
            Debug.LogError("Tried to attract multiple objects.");
            return;
        }

        if (this.connectionToClient != null && identity.connectionToClient != null)
            return;

        var rb = identity.GetComponent<Rigidbody>();
        if (rb == null)
        {
            return;
        }

        _localAttract = identity;
        if (this.connectionToClient != null)
        {
            identity.AssignClientAuthority(this.connectionToClient);
            Util.SetClientPhysicsAuthority(identity, true);
            TargetRpcBeginAttract(identity);
        }
        else // Server owned object
        {
            StartCoroutine(PullObject(rb));
        }
        
    }

    [Command]
    void CmdStopAttract(NetworkIdentity identity, Vector3 position, Vector3 direction, float chargeProgress)
    {
        attractVFX.GetComponent<VisualEffect>().Stop();
        if (identity == null)
            return;

        identity.RemoveClientAuthority();
        Util.SetClientPhysicsAuthority(identity, false);
        var rb = identity.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.position = position;
            if(rb.isKinematic)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = false;
            }

            if (chargeProgress > 0)
            {
                float pushForce = Mathf.Lerp(MinPushForce, MaxPushForce, Mathf.Clamp01(chargeProgress));
                rb.AddForce(direction * pushForce, this.PushForceMode);
                pushVFX.GetComponent<VisualEffect>().Play();
            }
        }

        _localAttract = null;
    }

    void StopPulling(bool push)
    {
        _pulling = false;
        if (_localAttract != null)
        {
            _localAttract.transform.parent = null;
            var rb = _localAttract.GetComponent<Rigidbody>();
            if (rb != null)
            {
                holdVFX.GetComponent<VisualEffect>().enabled = false;
                rb.isKinematic = false;
            }
            var collider = _localAttract.GetComponent<Collider>();
            if (collider != null)
                collider.enabled = true;

            Util.SetClientPhysicsAuthority(_localAttract, false);

            float chargeProgress = 0f;
            if(push && _charging)
            {
                chargeProgress = Mathf.Clamp01((float)_chargeBegan.Elapsed);
            }
            CmdStopAttract(_localAttract, _localAttract.transform.position, AnchorPoint.transform.forward, chargeProgress);
            _localAttract = null;
        }
    }

    void IWeapon.SecondaryAttack(bool isPressed)
    {
        StopPulling(false);
        if (isPressed)
        {
            var aimTransform = Util.GetOwnerAimTransform(this.GetComponent<NetworkItem>());
            if (Physics.SphereCast(aimTransform.position, 0.5f, aimTransform.forward, out RaycastHit hitResult, MaxGrabRange, TargetMask))
            {
                _pulling = true;
                var networkIdentity = hitResult.collider.GetComponent<NetworkIdentity>();
                CmdAttract(networkIdentity);
            }
        }
    }

    [Command]
    void CmdPush(NetworkIdentity identity, Vector3 localPosition, Vector3 localDirection, float chargeProgress)
    {
        if (identity == null || identity.connectionToClient != null)
        {
            return;
        }

        var rb = identity.GetComponent<Rigidbody>();
        if (rb != null)
        {
            if (chargeProgress > 0)
            {
                float pushForce = Mathf.Lerp(MinPushForce, MaxPushForce, Mathf.Clamp01(chargeProgress));

                var direction = rb.transform.TransformDirection(localDirection);
                var position = rb.transform.TransformPoint(localPosition);
                
                rb.AddForceAtPosition(direction * pushForce, position, this.PushForceMode);
            }
        }


    }

    void IWeapon.PrimaryAttack(bool isPressed)
    {
        if (isPressed)
        {
            _charging = true;
            _chargeBegan = NetworkTimer.Now;
        }
        else if (_charging)
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/PlaceholderAudio/Weapons/FPS_Microgame/Launcher/LauncherRelease");

            var rb = _localAttract?.GetComponent<Rigidbody>();
            if (rb != null && rb.isKinematic)
            {
                StopPulling(true);
            }
            else if (Physics.SphereCast(AnchorPoint.position, 0.5f, AnchorPoint.forward, out RaycastHit hitResult, MaxRange, TargetMask))
            {
                var networkIdentity = hitResult.collider.GetComponent<NetworkIdentity>();

                if (networkIdentity != null)
                {
                    if (_localAttract == networkIdentity)
                    {
                        StopPulling(true);
                    }
                    else
                    {
                        var t = networkIdentity.transform;
                        var localPosition = t.InverseTransformPoint(hitResult.point);
                        var localDirection = t.InverseTransformDirection(AnchorPoint.forward);                        
                        CmdPush(networkIdentity, localPosition, localDirection, Mathf.Clamp01((float)_chargeBegan.Elapsed));
                        pushVFX.GetComponent<VisualEffect>().Play();
                    }
                }
            }
            StopCharging();
        }
    }

    void StopCharging()
    {
        _charging = false;
    }

    float? IWeapon.ChargeProgress => _charging ? Mathf.Clamp01((float)_chargeBegan.Elapsed) : null;

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
        StopCharging();
        StopPulling(false);
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
        StopCharging();
        StopPulling(false);
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

