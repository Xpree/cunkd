using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class IceGadget : NetworkBehaviour, IGadget
{
    public GameObject IceGadgetTrap;
    Vector3 aimDirection;
    Vector3 aimPos;
    Vector3 target;
    [SerializeField] LayerMask TargetMask = ~0;

    [SyncVar] [SerializeField] int Charges;
    [SyncVar] public int chargesLeft;
    [SerializeField] bool isPassive;

    [SyncVar] float ChargeProgress = 0f;

    bool IGadget.isPassive => isPassive;
    int IGadget.Charges => Charges;
    int IGadget.ChargesLeft => chargesLeft;
    private void Awake()
    {
        chargesLeft = Charges;
    }
    [Client]
    void CmdPrimaryUse(bool isPressed, Vector3 direction, Vector3 position)
    {
        if (0 < chargesLeft)
        {
            aimDirection = direction;
            aimPos = position;
            target = FindTarget();
            if(target != new Vector3(0, 0, 0))
            {
                SpawnIceGadget(target);
                chargesLeft--;
            }
        }
        if (chargesLeft <= 0)
        {
            print("gadget out of charges");
        }
    }

    [Server]
    void SpawnIceGadget(Vector3 target)
    {
        var go = Instantiate(IceGadgetTrap, target, Quaternion.identity);
        NetworkServer.Spawn(go);
    }

    [Client]
    void CmdSecondaryUse()
    {
        
    }

    [Server]
    Vector3 FindTarget()
    {
        //Raycast target
        RaycastHit hitResult;
        if (Physics.Raycast(aimPos, aimDirection, out hitResult))
        {
            return hitResult.point;
        }
        return new Vector3(0, 0, 0);
    }

    void IGadget.PrimaryUse(bool isPressed)
    {
        CmdPrimaryUse(isPressed, Camera.main.transform.forward, Camera.main.transform.position);
    }

    void IGadget.SecondaryUse(bool isPressed)
    {
        CmdSecondaryUse();
    }

    float? IGadget.ChargeProgress => this.ChargeProgress;
}
