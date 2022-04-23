using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

[RequireComponent(typeof(NetworkItem))]
public class GravityGun : NetworkBehaviour, IWeapon, IEquipable
{
    Vector3 aimDirection;
    Vector3 aimPos;

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

    Rigidbody GrabTarget;
    Collider GrabTargetCollider;
    float GrabPullTime;
    float GrabTargetRadius;

    Vector3 AnchorPos => AnchorPoint.position + AnchorPoint.forward * GrabTargetRadius; //may cause issues with holding larger objects if not implemented

    bool Charging = false;
    bool Push = false;
    [SyncVar] float ChargeProgress = 0f;

    private void Start()
    {
        if (_settings == null)
        {
            Debug.LogError("Missing GameSettings reference on " + name);
        }
    }
    void IWeapon.initializeOnPlayer(Inventory player)
    {
    }

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
            //GrabTargetCollider.enabled = false;

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
                GrabTargetCollider.enabled = false;
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
        float searchRange = isPush ? MaxRange : MaxGrabRange;

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

    bool holstered = false;
    bool IEquipable.IsHolstered => holstered;

    System.Collections.IEnumerator testAnimation()
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
        StartCoroutine(testAnimation());
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
        this.transform.parent = null;
        if (holstered)
        {
            holstered = false;
            transform.localScale = Vector3.one;
        }

        PlayerCollider = null;
    }

    void IEquipable.OnRemoved()
    {
        this.transform.parent = null;
        if (holstered)
        {
            holstered = false;
            transform.localScale = Vector3.one;
        }
        PlayerCollider = null;
    }

}

