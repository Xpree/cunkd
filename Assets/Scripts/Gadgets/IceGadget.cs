using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkItem))]
public class IceGadget : NetworkBehaviour
{
    public GameObject IceGadgetTrap;
    [SerializeField] LayerMask TargetMask = ~0;
    NetworkItem _item;
    bool _used = false;

    void Awake()
    {
        _item = GetComponent<NetworkItem>();
        _item.ItemType = ItemType.Gadget;
    }

    [Command]
    void SpawnIceGadget(Vector3 target)
    {
        if (_used)
            return;
        _used = true;
        var go = Instantiate(IceGadgetTrap, target, Quaternion.identity);
        NetworkServer.Spawn(go);
        NetworkServer.Destroy(this.gameObject);
    }

    public bool Use()
    {
        if (_used)
            return false;
        _used = true;

        var aimTransform = GetComponent<NetworkItem>().OwnerInteractAimTransform;
        if (Util.RaycastPoint(aimTransform, 100.0f, TargetMask, out Vector3 point))
        {
            SpawnIceGadget(point);
        }
        else
        {
            SpawnIceGadget(_item.Owner.transform.position);
        }
        return true;
    }

}
