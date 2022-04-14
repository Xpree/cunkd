using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BlackHoleGun : NetworkBehaviour, IWeapon
{
    public GameObject blackHole;
    [SerializeField] float cooldown = 30f;
    [SerializeField] float timer;
    [SerializeField] float range = 20f;
    [SerializeField] LayerMask TargetMask = ~0;

    Vector3 target;
    Vector3 endTarget;

    bool hasFired = false;

    void IWeapon.initializeOnPlayer(Inventory player)
    {
    }

    [Command]
    public void CmdPrimaryAttack()
    {
        if (hasFired == false)
        {
            target = FindTarget();
            hasFired = true;
            SpawnBlackHole(target);
        }

    }
    [Server]
    void SpawnBlackHole(Vector3 target)
    {
        Instantiate(blackHole, target, Quaternion.identity);
    }
    [Server]
    Vector3 FindTarget()
    {

        //Raycast target
        RaycastHit hitResult;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hitResult, range, TargetMask))
        {
            return hitResult.point;
        }
        else
        {
            endTarget = transform.parent.parent.forward * range;
            Debug.Log("endtarget = " + endTarget);
            return (endTarget + transform.parent.parent.position);
        }
    }
    [ServerCallback]
    // Update is called once per frame
    void Update()
    {
        if (hasFired == true)
        {
            timer = timer + Time.deltaTime;
            if (timer >= cooldown)
            {
                hasFired = false;
                timer = 0;
            }
        }
    }

    void IWeapon.PrimaryAttack(bool isPressed)
    {
        CmdPrimaryAttack();
    }

    void IWeapon.SecondaryAttack(bool isPressed)
    {
        
    }

    float? IWeapon.ChargeProgress => null;
}