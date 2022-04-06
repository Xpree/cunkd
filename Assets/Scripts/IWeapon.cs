public interface IWeapon
{
    // The primary attack of the weapon (Left Mouse)
    void PrimaryAttack(bool isPressed);

    // The secondary attack of the weapon (Right Mouse)
    void SecondaryAttack(bool isPressed);


    // The charging progress of the weapon in range of [0,1]. Returns
    // null if the weapon is not capable of charging.
    float? ChargeProgress { get; }
}
