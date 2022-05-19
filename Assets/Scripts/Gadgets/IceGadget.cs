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

    [SyncVar(hook = nameof(OnPosition))] public Vector3 positionSync;

    bool IGadget.isPassive => isPassive;
    int IGadget.Charges => _settings.IceGadget.Charges;
    int IGadget.ChargesLeft => _cooldownTimer.Charges;

    [SerializeField] float Cooldown;
    GameObject trap;

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
    public void sync(Vector3 pos)
    {
        positionSync = pos;
    }

    [Client]
    void OnPosition(Vector3 previous, Vector3 current)
    {
        trap.transform.position = current;
        trap.GetComponent<Rigidbody>().isKinematic = true;
        trap.GetComponent<IceGadgetTrap>().iceMachine.Trigger();
    }

    [Command]
    void CmdSpawnIceGadget()
    {
        RpcSpawnIceGadgetClient();
    }

    [ClientRpc(includeOwner = false)]
    void RpcSpawnIceGadgetClient()
    {
        InstantiateIceTrap();
    }

    void InstantiateIceTrap(IceGadget ig = null)
    {
        trap = Instantiate(IceGadgetTrap, transform.position + transform.forward * 3, Quaternion.identity);
        trap.GetComponent<Rigidbody>().AddForce(transform.forward * throwForce * 10);
        trap.GetComponent<Rigidbody>().AddTorque(new Vector3(0, 100000, 0), ForceMode.Force);
        trap.GetComponent<IceGadgetTrap>().owner = ig;
    }

    [Command]
    void DestroyGadget()
    {
        NetworkServer.Destroy(this.gameObject);
    }

    void IGadget.PrimaryUse(bool isPressed)
    {
        if (isPressed == false)
            return;

        if (_cooldownTimer.Use())
        {
            AudioHelper.PlayOneShotAttachedWithParameters("event:/SoundStudents/SFX/Gadgets/Icy Floor Trap", this.gameObject, ("Shot", 1f), ("StandbyHum", 1f));
            InstantiateIceTrap(this);
            CmdSpawnIceGadget();
            if (_cooldownTimer.Charges <= 0)
            {
                Invoke("DestroyGadget", _cooldownTimer.CooldownDuration);
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
