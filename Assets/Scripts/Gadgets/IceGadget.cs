using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkItem))]
[RequireComponent(typeof(NetworkCooldown))]
public class IceGadget : NetworkBehaviour, IGadget, IEquipable
{
    public GameObject IceGadgetTrap;

    [SerializeField] int Charges;
    [SerializeField] bool isPassive;
    [SerializeField] LayerMask TargetMask = ~0;


    NetworkCooldown _cooldownTimer;

    bool IGadget.isPassive => isPassive;
    int IGadget.Charges => Charges;
    int IGadget.ChargesLeft => _cooldownTimer.Charges;
    
    void Awake()
    {
        _cooldownTimer = GetComponent<NetworkCooldown>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        _cooldownTimer.SetCharges(Charges);
    }

    [Command]
    void SpawnIceGadget(Vector3 target)
    {
        if(_cooldownTimer.ServerUseCharge())
        {
            var go = Instantiate(IceGadgetTrap, target, Quaternion.identity);
            NetworkServer.Spawn(go);

            if(_cooldownTimer.Charges == 0)
            {
                NetworkServer.Destroy(this.gameObject);
            }
        }
    }

    void IGadget.PrimaryUse(bool isPressed)
    {
        if (isPressed == false || _cooldownTimer.UseCharge() == false)
            return;

        var aimTransform = GetComponent<NetworkItem>().OwnerInteractAimTransform;
        if (aimTransform == null)
        {
            Debug.LogError("Aim transform missing.");
            return;
        }

        if (Util.RaycastPoint(aimTransform, 100.0f, TargetMask, out Vector3 point))
        {
            SpawnIceGadget(point);
        }
    }

    void IGadget.SecondaryUse(bool isPressed)
    {
    }


    float? IGadget.ChargeProgress => null;


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
