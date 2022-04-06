public interface IWeapon
{
    void PrimaryAttack(bool isPressed);
    void SecondaryAttack(bool isPressed);
    float? ChargeProgress { get; }
}
