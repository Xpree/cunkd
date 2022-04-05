using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class GravityGun : NetworkBehaviour
{
    

    [SerializeField] Transform AnchorPoint;
    [SerializeField] LayerMask TargetMask = ~0;

    //grab
    [SerializeField] float MaxGrabRange = 40f;
    [SerializeField] float GrabTime = 0.5f;
    [SerializeField] float GrabTorque = 10f;
    [SerializeField] Collider PlayerCollider;

    //push
    [SerializeField] float MinPushForce = 10f;
    [SerializeField] float MaxPushForce = 100f;
    [SerializeField] float ChargeRate = 1f;
    [SerializeField] float MaxRange = 30f;
    [SerializeField] ForceMode PushForceMode = ForceMode.Impulse;

    Rigidbody GrabTarget;
    Collider GrabTargetCollider;
    float GrabPullTime;
    float GrabTargetRadius;

    Vector3 AnchorPos => AnchorPoint.position + AnchorPoint.forward * GrabTargetRadius;

    bool Charging = false;
    bool Push = false;
    float ChargeProgress = 0f;

    [Command]
    void PrimaryAttack(bool isPressed)
    {
        if (!isPressed)
        {
            Push = true;
            return;
        }
        Debug.Log("primary attack");
        Charging = true;
        ChargeProgress = 0f;
        Push = false;
    }
    [Command]
    void SecondaryAttack(bool isPressed)
    {
        if (!isPressed)
        {
            if (GrabTarget != null)
            {
                ClearGrabTarget();
            }
            return;
        }
        Debug.Log("secondary attack");
        GrabTarget = FindTarget(false);
        Debug.Log($"Grabbed target = {GrabTarget}");
        if (GrabTarget != null)
        {
            //if change collision mode do it here
            GrabTargetCollider = GrabTarget.GetComponent<Collider>();
            GrabTargetRadius = GrabTargetCollider.bounds.extents.magnitude;
            GrabPullTime = 0f;
            if (GrabTorque != 0f)
            {
                GrabTarget.AddTorque(Random.Range(-GrabTorque, GrabTorque), Random.Range(-GrabTorque, GrabTorque), Random.Range(-GrabTorque, GrabTorque));
            }
            //turns on collisions with play, turn on with map too?
            Physics.IgnoreCollision(GrabTargetCollider, PlayerCollider, true);

        }
    }

    void GravGunPullnShot()
    {
        if (Charging && Push)
        {
            Charging = Push = false;

            Rigidbody targetRB = GrabTarget != null ? GrabTarget : FindTarget(true);
            if (targetRB != null)
            {
                if (GrabTarget != null)
                {
                    ClearGrabTarget();
                }
                float pushForce = Mathf.Lerp(MinPushForce, MaxPushForce, ChargeProgress);
                targetRB.AddForce(Camera.main.transform.forward * pushForce, PushForceMode);
            }
        }

        else if (GrabTarget != null && GrabPullTime < GrabTime)
        {
            GrabPullTime += Time.deltaTime;

            if (GrabPullTime >= GrabTime)
            {
                GrabTarget.velocity = Vector3.zero;
                GrabTarget.transform.position = AnchorPos;
                GrabTarget.isKinematic = true;
                GrabTarget.transform.SetParent(AnchorPoint);
            }
            else
            {
                float timeToAnchor = GrabTime - GrabPullTime;
                GrabTarget.velocity = (AnchorPos - GrabTarget.transform.position) / timeToAnchor;
            }
        }
    }

    void GravGunCharge()
    {
        if (Charging == true && ChargeProgress < 1f)
        {
            ChargeProgress = Mathf.Clamp01(ChargeProgress + ChargeRate * Time.deltaTime);
        }
    }

    void ClearGrabTarget()
    {
        //de-parent
        if (GrabTarget.transform.parent == AnchorPoint)
        {
            GrabTarget.transform.SetParent(null);
        }

        GrabTarget.isKinematic = false;
        //turns off collisions with play, turn off with map too?
        Physics.IgnoreCollision(GrabTargetCollider, PlayerCollider, false);

        //clear target
        GrabTarget = null;
        GrabTargetCollider = null;
    }

    Rigidbody FindTarget(bool isPush)
    {
        float searchRange = isPush ? MaxPushForce : MaxGrabRange;

        //Raycast target
        RaycastHit hitResult;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hitResult, searchRange, TargetMask))
        {
            return hitResult.collider.GetComponent<Rigidbody>();
        }
        return null;
    }

    
    void Update()
    {
        if (this.isServer)
            GravGunCharge();

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            PrimaryAttack(true);
        }
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            PrimaryAttack(false);
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            SecondaryAttack(true);
        }
        if (Mouse.current.rightButton.wasReleasedThisFrame)
        {
            SecondaryAttack(false);
        }
    }

    void FixedUpdate()
    {
        if (this.isServer)
            GravGunPullnShot();
    }
}

