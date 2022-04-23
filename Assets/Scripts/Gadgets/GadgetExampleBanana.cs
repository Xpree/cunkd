using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkItem))]
public class GadgetExampleBanana : NetworkBehaviour, IGadget, IEquipable
{
    [SyncVar][SerializeField] int Charges;
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
    void CmdPrimaryUse()
    {
        if (0 < chargesLeft)
        {
            print("ate a piece of the banana");
            chargesLeft--;
        }
        if (chargesLeft <= 0)
        {
            print("gadget out of charges");
        }
    }

    [Client]
    void CmdSecondaryUse()
    {
        if (0 < chargesLeft)
        {
            print("ate some other piece of the banana");
            chargesLeft--;
        }
        if (chargesLeft <= 0)
        {
            print("gadget out of charges");
        }
    }

    void IGadget.PrimaryUse(bool isPressed)
    {
        CmdPrimaryUse();
    }

    void IGadget.SecondaryUse(bool isPressed)
    {
        CmdSecondaryUse();
    }

    float? IGadget.ChargeProgress => this.ChargeProgress;


    bool holstered;
    bool IEquipable.IsHolstered => holstered;

    void IEquipable.OnHolstered()
    {
        // TODO Animation then set holstered
        holstered = true;
        transform.localScale = Vector3.zero;
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
