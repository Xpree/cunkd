using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class GravityGun : NetworkBehaviour, IWeapon
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

    float? IWeapon.ChargeProgress => throw new System.NotImplementedException();

    bool Charging = false;
    bool Push = false;
    [SyncVar] float ChargeProgress = 0f;


    [Command]
    void CmdPrimaryAttack(bool isPressed)
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
    void CmdSecondaryAttack(bool isPressed)
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
                targetRB.AddForce(Camera.main.transform.forward * pushForce, PushForceMode);
            }

            ChargeProgress = 0;
        }

        else if (GrabTarget != null && GrabPullTime < GrabTime)
        {
            GrabPullTime += Time.fixedDeltaTime;

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

    [Server]
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
    
    [ServerCallback]
    void FixedUpdate()
    {
        GravGunCharge();
        GravGunPullnShot();
    }


    private void OnGUI()
    {
        if (!isLocalPlayer)
            return;

        if(ChargeProgress> 0) {
            GUI.Box(new Rect(Screen.width * 0.5f - 50, Screen.height * 0.8f - 10, 100.0f * ChargeProgress, 20.0f), GUIContent.none);
        }
    }

    void IWeapon.PrimaryAttack(bool isPressed)
    {
        CmdPrimaryAttack(isPressed);
    }

    void IWeapon.SecondaryAttack(bool isPressed)
    {
        CmdSecondaryAttack(isPressed);
    }
}

