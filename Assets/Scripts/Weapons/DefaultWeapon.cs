using Mirror;

public class DefaultWeapon : NetworkBehaviour, IWeapon
{
    void IWeapon.PrimaryAttack(bool isPressed) { CmdPrimaryAttack(true); }

    void IWeapon.SecondaryAttack(bool isPressed) { }

    float? IWeapon.ChargeProgress => null;

    [Command]
    void CmdPrimaryAttack(bool isPressed)
    {
        print("prim attack");

    }
}
