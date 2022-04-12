using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class GravityGun : NetworkBehaviour, IWeapon
{
    Vector3 aimDirection;
    Vector3 aimPos;

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

    Vector3 AnchorPos => AnchorPoint.position; // + AnchorPoint.forward * GrabTargetRadius; //may cause issues with holding larger objects if not implemented

    bool Charging = false;
    bool Push = false;
    [SyncVar] float ChargeProgress = 0f;


    [Command]
    void CmdPrimaryAttack(bool isPressed, Vector3 direction, Vector3 position)
    {
        aimDirection = direction;
        aimPos = position;
        if (!isPressed)
        {
            Push = true;
            return;
        }
        Charging = true;
        ChargeProgress = 0f;
        Push = false;
    }
    
    [Command]
    void CmdSecondaryAttack(bool isPressed, Vector3 direction, Vector3 position)
    {
        aimDirection = direction;
        aimPos = position;
        if (!isPressed)
        {
            if (GrabTarget != null)
            {
                ClearGrabTarget();
            }
            return;
        }
        GrabTarget = FindTarget(false);
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
            //turns off collisions with play, turn on with map too?
            Physics.IgnoreCollision(GrabTargetCollider, PlayerCollider, true);
            GrabTargetCollider.enabled = false;

        }
    }

    [Server]
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
                targetRB.AddForce(aimDirection * pushForce, PushForceMode);
            }

            ChargeProgress = 0;
        }

        else if (GrabTarget != null)// && GrabPullTime < GrabTime)
        {
            GrabPullTime += Time.fixedDeltaTime;

            if (GrabPullTime >= GrabTime)
            {
                GrabTarget.velocity = Vector3.zero;
                GrabTarget.transform.position = AnchorPos;
                //GrabTarget.isKinematic = true;
                //GrabTarget.transform.SetParent(AnchorPoint);
            }
            else
            {
                float timeToAnchor = GrabTime - GrabPullTime;
                GrabTarget.velocity = (AnchorPos - GrabTarget.transform.position) / timeToAnchor;
            }
        }
    }

    [Server]
    void GravGunCharge()
    {
        if (Charging == true && ChargeProgress < 1f)
        {
            ChargeProgress = Mathf.Clamp01(ChargeProgress + ChargeRate * Time.fixedDeltaTime);
        }
    }

    [Server]
    void ClearGrabTarget()
    {
        //de-parent
        //if (GrabTarget.transform.parent == AnchorPoint)
        //{
        //    //GrabTarget.transform.SetParent(null);
        //}

        //GrabTarget.isKinematic = false;
        //turns on collisions with play, turn off with map too?
        GrabTargetCollider.enabled = true;
        Physics.IgnoreCollision(GrabTargetCollider, PlayerCollider, false);

        //clear target
        GrabTarget = null;
        GrabTargetCollider = null;
    }

    [Server]
    Rigidbody FindTarget(bool isPush)
    {
        float searchRange = isPush ? MaxPushForce : MaxGrabRange;

        //Raycast target
        RaycastHit hitResult;
        if (Physics.Raycast(aimPos, aimDirection, out hitResult, searchRange, TargetMask))
        {
            return hitResult.collider.GetComponent<Rigidbody>();
        }
        return null;
    }

    [ServerCallback]
    void FixedUpdate()
    {
        GravGunCharge();
        GravGunPullnShot();
    }

    void IWeapon.PrimaryAttack(bool isPressed)
    {
        CmdPrimaryAttack(isPressed, Camera.main.transform.forward, Camera.main.transform.position);
    }

    void IWeapon.SecondaryAttack(bool isPressed)
    {
        CmdSecondaryAttack(isPressed, Camera.main.transform.forward, Camera.main.transform.position);
    }

    float? IWeapon.ChargeProgress => this.ChargeProgress;
}

