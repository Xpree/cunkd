using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GadgetExampleBanana : NetworkBehaviour, IGadget
{
    [SyncVar] [SerializeField] int Charges;
    [SyncVar]int chargesLeft;
    [SerializeField] bool isPassive;

    [SyncVar] float ChargeProgress = 0f;

    bool IGadget.isPassive => isPassive;
    int IGadget.Charges => Charges;

    private void Awake()
    {
        chargesLeft = Charges;
    }

    void CmdPrimaryUse()
    {
        if (0 < chargesLeft)
        {
            print("ate a piece of the banana");
            chargesLeft--;

        }
        if (chargesLeft <= 0)
        {
            outOfCharges();
        }
    }
    void CmdSecondaryUse()
    {
        if (0 < chargesLeft)
        {
            print("ate some other piece of the banana");
            chargesLeft--;
        }
        if (chargesLeft <= 0)
        {
            outOfCharges();
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

    void outOfCharges()
    {
        print("gadget out of charges");
        NetworkServer.Destroy(gameObject);
    }

    float? IGadget.ChargeProgress => this.ChargeProgress;
}
