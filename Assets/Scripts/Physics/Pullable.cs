using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(Rigidbody))]
public class Pullable : NetworkBehaviour
{
    public GameObject target;
    float offTime;

    public Collider pullingCollider;
    public Rigidbody body;       
    bool pulling = false;

    public float pullOffset;
    NetworkTimer fixedTimer;

    public Vector3 TargetPosition => target.transform.position + pullOffset * target.transform.forward;
    public Vector3 offsetPosition;
    public bool IsFixed => pullingCollider.enabled == false;

    public bool IsBeingPulled => pulling && target != null && target.activeSelf;


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
            Util.SetPhysicsSynchronized(this.netIdentity, true);
            body.isKinematic = false;
            foreach (Collider c in GetComponents<Collider>())
            {
                c.enabled = true;
            }
            this.transform.parent = null;
        }
        else
        {
            Util.SetPhysicsSynchronized(this.netIdentity, false);
        }
    }

    void SetFixed()
    {
        if (pullingCollider.enabled)
        {
            foreach (Collider c in GetComponents<Collider>())
            {
                c.enabled = false;
            }
            if (Physics.Raycast(target.transform.position, target.transform.forward, out RaycastHit hit, pullOffset, GameServer.Instance.Settings.Movable))
            {
                if (hit.collider.gameObject == this.gameObject)
                {
                    offsetPosition = hit.point - target.transform.position;
                    this.transform.localPosition = -offsetPosition;
                }
                return;
            }

            body.isKinematic = true;
            
            body.transform.parent = target.transform;
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;

            Color color = this.GetComponent<Renderer>().material.color;
            color.a = 0.5f;
            this.gameObject.GetComponent<Renderer>().material.color = color;
            this.gameObject.GetComponent<KnockbackScript>().onOff = true;
            offTime = 5f;
        }
        //else
        //{
        //    body.transform.localPosition = Vector3.zero;
        //}
        //body.position = Vector3.Lerp(body.position, TargetPosition, 0.5f);
        
        
        body.position = Vector3.Lerp(body.position, TargetPosition, 0.5f);
    }




    public void StartPulling(GameObject destination, NetworkTimer timeToFixed)
    {
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
        if(offTime >= 0 && !pulling)
            offTime = offTime - 1 * Time.deltaTime;
        if (!pulling)
            return;
        
        if (target == null || target.activeSelf == false)
        {
            SetPulling(false);
            return;
        }

        var timeToAnchor = (float)fixedTimer.Remaining;
        if(timeToAnchor <= 0)
        {
            SetFixed();
            return;
        }
        
        body.velocity = (TargetPosition - body.position) / timeToAnchor;
    }

}
