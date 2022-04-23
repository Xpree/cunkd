using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkItem))]
public class BlackHoleGun : NetworkBehaviour, IWeapon, IEquipable
{
    Vector3 aimDirection;
    Vector3 aimPos;

    [SerializeField] GameSettings _settings;
    float cooldown => _settings.BlackHoleGun.Cooldown;
    float range => _settings.BlackHoleGun.Range;

    [SerializeField] GameObject blackHole;
    [SerializeField] LayerMask TargetMask = ~0;

    Vector3 target;
    Vector3 endTarget;
    float timer;

    bool hasFired = false;

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
        CmdPrimaryAttack(isPressed, Camera.main.transform.forward, Camera.main.transform.position);
    }

    void IWeapon.SecondaryAttack(bool isPressed)
    {

    }

    float? IWeapon.ChargeProgress => null;

    bool holstered;
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
    }

    void IEquipable.OnDropped()
    {
        this.transform.parent = null;
        if (holstered)
        {
            holstered = false;
            transform.localScale = Vector3.one;
        }
    }

    void IEquipable.OnRemoved()
    {
        this.transform.parent = null;
        if (holstered)
        {
            holstered = false;
            transform.localScale = Vector3.one;
        }
    }
}