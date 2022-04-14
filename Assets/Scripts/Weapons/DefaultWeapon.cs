using Mirror;

public class DefaultWeapon : NetworkBehaviour, IWeapon
{
    void IWeapon.initializeOnPlayer(Inventory player) { }

    void IWeapon.PrimaryAttack(bool isPressed) { }

    void IWeapon.SecondaryAttack(bool isPressed) { }

    float? IWeapon.ChargeProgress => null;
}
