using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(Rigidbody))]
public class Pullable : NetworkBehaviour
{
    public GameObject target;

    public Collider pullingCollider;
    public Rigidbody body;       
    bool pulling = false;

    float pullOffset = 0;
    NetworkTimer fixedTimer;

    public Vector3 TargetPosition => target.transform.position + pullOffset * target.transform.forward;
    public bool IsFixed => pullingCollider.enabled == false;

    public bool IsBeingPulled => pulling && target != null && target.activeSelf;


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
            pullingCollider.enabled = true;
            this.transform.parent = null;
        }
        else
        {
            Util.SetPhysicsSynchronized(this.netIdentity, false);
        }
    }

    void SetFixed()
    {
        if(pullingCollider.enabled)
        {
            pullingCollider.enabled = false;
            body.isKinematic = true;
            body.position = TargetPosition;
            body.transform.parent = target.transform;
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;            
        }
        else
        {
            body.transform.localPosition = Vector3.zero;
        }
    }


    public void StartPulling(GameObject destination, float offset, NetworkTimer timeToFixed)
    {
        target = destination;
        pullOffset = offset;
        fixedTimer = timeToFixed;
        SetPulling(true);

        Renderer rend = GetComponent<Renderer>();
        if (rend)
        {
            Color col = rend.material.color;
            rend.material.color = col * new Color(1, 1, 1, 0.5f);
        }
        print("hejhej");
    }

    public void StopPulling()
    {
        target = null;
        SetPulling(false);

    }

    void FixedUpdate()
    {
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
