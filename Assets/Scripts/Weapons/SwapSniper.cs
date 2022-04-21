using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SwapSniper : NetworkBehaviour, IWeapon
{

    Vector3 aimDirection;
    Vector3 aimPos;

    [SerializeField] GameSettings _settings;
    float cooldown => _settings.SwapSniper.Cooldown;
    float range => _settings.SwapSniper.Range;

    [SerializeField] LayerMask TargetMask = ~0;

    Rigidbody target;
    Vector3 endTarget;
    float timer;

    bool hasFired = false;
    PlayerMovement owner;

    private void Start()
    {
        if (_settings == null)
        {
            Debug.LogError("Missing GameSettings reference on " + name);
        }
    }

    void IWeapon.initializeOnPlayer(Inventory player)
    {
        owner = player.GetComponent<PlayerMovement>();
    }

    //Firing-function
    public void DoPrimaryAttack(bool isPressed, Vector3 direction, Vector3 position)
    {
        aimDirection = direction;
        aimPos = position;
        if (hasFired == false)
        {
            
            target = DidHitObject();
            hasFired = true;
            PerformSwap(target);
        }

    }
    
    //Checks if a rigidbody was hit
    public Rigidbody DidHitObject()
    {

        //Raycast target
        RaycastHit hitResult;
        if (Physics.Raycast(aimPos, aimDirection, out hitResult, range, TargetMask))
        {
            return hitResult.rigidbody;
        }
        else
        {
            return null;
        }
    }

    
    void PerformSwap(Rigidbody target)
    {
        if (target != null)
        {
            Vector3 Swapper = owner.transform.position;
            Vector3 Swappee = target.position;

            Util.Teleport(target.gameObject, Swapper);
            Util.Teleport(owner.gameObject, Swappee);
        }
    }

    [ClientCallback]
    // Update is called once per frame
    void FixedUpdate()
    {
        if (hasFired == true)
        {
            timer = timer + Time.fixedDeltaTime;
            if (timer >= cooldown)
            {
                hasFired = false;
                timer = 0;
            }
        }
    }

    void IWeapon.PrimaryAttack(bool isPressed)
    {
        DoPrimaryAttack(isPressed, Camera.main.transform.forward, Camera.main.transform.position);
    }

    void IWeapon.SecondaryAttack(bool isPressed)
    {

    }

    float? IWeapon.ChargeProgress => null;
}
