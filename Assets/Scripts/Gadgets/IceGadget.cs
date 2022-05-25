using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkItem))]
[RequireComponent(typeof(NetworkCooldown))]
public class IceGadget : NetworkBehaviour, IGadget, IEquipable
{
    [SerializeField] bool isPassive;

    [SerializeField] LayerMask TargetMask = ~0;
    float throwForce;

    [SerializeField] GameSettings _settings;
    NetworkCooldown _cooldownTimer;

    bool IGadget.isPassive => isPassive;
    int IGadget.Charges => _settings.IceGadget.Charges;
    int IGadget.ChargesLeft => _cooldownTimer.Charges;

    [SerializeField] float Cooldown;
    public GameObject IceTrapHub;
    IceTrapHub hub;

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

    //[Command]
    void DestroyGadget()
    {
        hub.ActivateSelfDestruction();
        NetworkServer.Destroy(this.gameObject);
    }



    void spawnHub()
    {
        GameObject hubObject = Instantiate(IceTrapHub, transform.position, Quaternion.identity);
        NetworkServer.Spawn(hubObject, this.connectionToClient);
        hub = hubObject.GetComponent<IceTrapHub>();
        setHub();
    }

   [ClientRpc]
    void setHub()
    {
        hub = FindObjectOfType<IceTrapHub>();
    }

    void IGadget.PrimaryUse(bool isPressed)
    {
        if (isPressed == false)
            return;

        if (_cooldownTimer.Use())
        {           
            hub.InstantiateIceTrap(transform.position + transform.forward, transform.forward * throwForce * 10, this);
            hub.CmdSpawnIceGadget(transform.position + transform.forward, transform.forward * throwForce * 10);

            if (_cooldownTimer.Charges <= 0)
            {
                DestroyGadget();
            }
        }
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
        if (isServer)
        {
            spawnHub();
        }

        holstered = startHolstered;

        if (holstered)
            transform.localScale = Vector3.zero;
        else
            transform.localScale = Vector3.one;
    }

    void IEquipable.OnDropped()
    {
        DestroyGadget();
        this.transform.parent = null;
        if (holstered)
        {
            holstered = false;
            transform.localScale = Vector3.one;
        }
    }

    #endregion
}
