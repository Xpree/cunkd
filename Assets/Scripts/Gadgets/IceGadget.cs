using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkItem))]
[RequireComponent(typeof(NetworkCooldown))]
public class IceGadget : NetworkBehaviour, IGadget, IEquipable
{
    public GameObject IceGadgetTrap;

    [SerializeField] bool isPassive;
    [SerializeField] LayerMask TargetMask = ~0;
    float throwForce;

    [SerializeField] GameSettings _settings;
    NetworkCooldown _cooldownTimer;

    bool IGadget.isPassive => isPassive;
    int IGadget.Charges => _settings.IceGadget.Charges;
    int IGadget.ChargesLeft => _cooldownTimer.Charges;

    [SerializeField] float Cooldown;
    
    void Awake()
    {
        _cooldownTimer = GetComponent<NetworkCooldown>();
        _cooldownTimer.CooldownDuration = _settings.IceGadget.Cooldown;
        throwForce = _settings.IceGadget.ThrowForce;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        if (_settings == null)
        {
            Debug.LogError("Missing GameSettings reference on " + name);
        }
        _cooldownTimer.SetCharges(_settings.IceGadget.Charges);
    }

    [Command]
    void SpawnIceGadget()
    {
        if (_cooldownTimer.ServerUse(_settings.IceGadget.Cooldown))
        {
            AudioHelper.PlayOneShotWithParameters("event:/SoundStudents/SFX/Gadgets/Icy Floor Trap", this.transform.position, ("Shot", 1f), ("StandbyHum", 1f));

            var go = Instantiate(IceGadgetTrap, transform.position + transform.forward *3, Quaternion.identity);
            go.GetComponent<Rigidbody>().AddForce(transform.forward * throwForce *10);
            go.GetComponent<Rigidbody>().AddTorque(new Vector3(0, 100000, 0), ForceMode.Force);
            NetworkServer.Spawn(go);

            if (_cooldownTimer.Charges == 0)
            {
                NetworkServer.Destroy(this.gameObject);
            }
        }
    }


    void IGadget.PrimaryUse(bool isPressed)
    {
        if (isPressed == false)
            return;
               
        SpawnIceGadget();
    }

    void IGadget.SecondaryUse(bool isPressed)
    {
    }


    float? IGadget.ChargeProgress => 1;


    #region IEquipable
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

    #endregion
}
