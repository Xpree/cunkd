public interface IGadget
{

    int Charges { get; }

    int ChargesLeft { get; }

    bool isPassive { get; }
    // The primary use of the gadget
    void PrimaryUse(bool isPressed);

    // The secondary use of the gadget
    void SecondaryUse(bool isPressed);


    // The charging progress of the gadget in range of [0,1]. Returns
    // null if the weapon is not capable of charging.
    float? ChargeProgress { get; }
}
