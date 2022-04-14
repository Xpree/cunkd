using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BlackHoleGun : NetworkBehaviour, IWeapon
{
    Vector3 aimDirection;
    Vector3 aimPos;

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
    public void CmdPrimaryAttack(bool isPressed, Vector3 direction, Vector3 position)
    {
        aimDirection = direction;
        aimPos = position;
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
        var go = Instantiate(blackHole, target, Quaternion.identity);
        NetworkServer.Spawn(go);
    }
    [Server]
    Vector3 FindTarget()
    {

        //Raycast target
        RaycastHit hitResult;
        if (Physics.Raycast(aimPos, aimDirection, out hitResult, range, TargetMask))
        {
            return hitResult.point;
        }
        else
        {
            endTarget = aimDirection * range;
            return (endTarget + aimPos);
        }
    }
    [ServerCallback]
    // Update is called once per frame
    void FixedUpdate()
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
        CmdPrimaryAttack(isPressed, Camera.main.transform.forward, Camera.main.transform.position);
    }

    void IWeapon.SecondaryAttack(bool isPressed)
    {
        
    }

    float? IWeapon.ChargeProgress => null;
}