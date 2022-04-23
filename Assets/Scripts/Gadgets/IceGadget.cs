using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkItem))]
public class IceGadget : NetworkBehaviour, IGadget, IEquipable
{
    public GameObject IceGadgetTrap;

    [SerializeField] LayerMask TargetMask = ~0;

    [SyncVar][SerializeField] int Charges;
    [SyncVar] public int chargesLeft;
    [SerializeField] bool isPassive;

    [SyncVar] float ChargeProgress = 0f;

    bool IGadget.isPassive => isPassive;
    int IGadget.Charges => Charges;
    int IGadget.ChargesLeft => chargesLeft;
    private void Awake()
    {
        chargesLeft = Charges;
    }

    [Command]
    void SpawnIceGadget(Vector3 target)
    {
        if (0 < chargesLeft)
        {
            var go = Instantiate(IceGadgetTrap, target, Quaternion.identity);
            NetworkServer.Spawn(go);
            chargesLeft--;
        }
    }

    void IGadget.PrimaryUse(bool isPressed)
    {
        if (chargesLeft <= 0)
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

    float? IGadget.ChargeProgress => this.ChargeProgress;


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

    void IEquipable.OnRemoved()
    {
        this.transform.parent = null;
        if (holstered)
        {
            holstered = false;
            transform.localScale = Vector3.one;
        }
    }
}
