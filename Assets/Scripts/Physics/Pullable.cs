using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(Rigidbody))]
public class Pullable : NetworkBehaviour
{
    public GameObject target;
    public float offTime;

    public Collider pullingCollider;
    public Rigidbody body;
    bool pulling = false;

    public float pullOffset;
    NetworkTimer fixedTimer;

    public Vector3 TargetPosition => target.transform.position + pullOffset * target.transform.forward;
    public Vector3 offsetPosition;
    public bool IsFixed => pullingCollider.enabled == false;

    public bool IsBeingPulled => pulling && target != null && target.activeSelf;

    public bool CanBePulled => IsBeingPulled == false && pullCooldown.Elapsed > 0;

    NetworkTimer pullCooldown;

    [ClientRpc]
    void RpcSetCooldown(NetworkTimer timer)
    {
        pullCooldown = timer;
    }

    [Server]
    public void SetCooldown()
    {
        pullCooldown = NetworkTimer.FromNow(2.0);
        RpcSetCooldown(pullCooldown);
    }


    private void Start()
    {
        this.gameObject.GetComponent<KnockbackScript>().onOff = false;
        var bounds = pullingCollider.bounds;
        foreach (Collider c in GetComponents<Collider>())
        {
            bounds.Encapsulate(c.bounds);
        }
        pullOffset = bounds.extents.magnitude;
    }

    private void OnValidate()
    {
        if (pullingCollider == null)
            pullingCollider = GetComponent<Collider>();
        if (body == null)
            body = GetComponent<Rigidbody>();
    }


    void SetPulling(bool value)
    {
        if (pulling == value)
            return;
        pulling = value;


        if (!pulling)
        {
            SetTransparent(false);
            Util.SetPhysicsSynchronized(this.netIdentity, true);

            if (this.isServer)
            {
                var collisions = Physics.OverlapSphere(this.transform.position, 0.05f, GameServer.Instance.Settings.DespawnPullableMask, QueryTriggerInteraction.Ignore);
                bool violation = false;
                foreach(var c in collisions)
                {
                    if (c.gameObject == this.gameObject)
                        continue;
                    violation = true;
                    break;
                }
                if (violation)
                {
                    NetworkServer.Destroy(this.gameObject);
                }
            }
        }
        else
        {
            Util.SetPhysicsSynchronized(this.netIdentity, false);
        }
    }

    void SetTransparent(bool enable)
    {
        var layer = LayerMask.NameToLayer(enable ? "Held" : "Movable");
        this.gameObject.layer = layer;
        foreach (Transform r in GetComponentsInChildren<Transform>(true))
        {
            r.gameObject.layer = layer;
        }

        foreach (Collider c in GetComponents<Collider>())
        {
            c.enabled = !enable;
        }

        if (enable)
        {
            body.isKinematic = true;
            body.transform.parent = target.transform;
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
        else
        {
            body.isKinematic = false;
            body.transform.parent = null;
        }
    }

    void SetFixed()
    {
        if (pullingCollider.enabled)
        {
            /*
            if (Physics.Raycast(target.transform.position, target.transform.forward, out RaycastHit hit, pullOffset, GameServer.Instance.Settings.Movable))
            {
                if (hit.collider.gameObject == this.gameObject)
                {
                    offsetPosition = hit.point - target.transform.position;
                    this.transform.localPosition = -offsetPosition;
                }
                return;
            }
            */
            SetTransparent(true);
            this.gameObject.GetComponent<KnockbackScript>().onOff = true;
            offTime = 5f;
        }
        //else
        //{
        //    body.transform.localPosition = Vector3.zero;
        //}
        //body.position = Vector3.Lerp(body.position, TargetPosition, 0.5f);


        transform.position = Vector3.Lerp(transform.position, TargetPosition, 0.5f);
    }

    public void StartPulling(GameObject destination, NetworkTimer timeToFixed)
    {
        this.gameObject.GetComponent<KnockbackScript>().onOff = true;
        offTime = 5f;
        target = destination;
        offsetPosition = Vector3.zero;
        fixedTimer = timeToFixed;
        SetPulling(true);
    }

    public void StopPulling()
    {
        target = null;
        SetPulling(false);
    }

    void FixedUpdate()
    {
        if (!pulling && offTime <= 0)
        {
            this.gameObject.GetComponent<KnockbackScript>().onOff = false;
        }

        if (offTime >= 0 && !pulling)
            offTime = offTime - Time.fixedDeltaTime;
        if (!pulling)
            return;

        if (target == null || target.activeSelf == false)
        {
            SetPulling(false);
            return;
        }

        var timeToAnchor = (float)fixedTimer.Remaining;
        if (timeToAnchor <= 0)
        {
            SetFixed();
            return;
        }

        body.velocity = (TargetPosition - body.position) / timeToAnchor;
    }

}
