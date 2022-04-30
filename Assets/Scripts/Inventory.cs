using UnityEngine;
using Mirror;
using Unity.VisualScripting;

public class Inventory : NetworkBehaviour, INetworkItemOwner
{
    [SerializeField] public Transform primaryWeaponAnchor;
    [SerializeField] public Transform secondaryWeaponAnchor;
    [SerializeField] public Transform gadgetAnchor;
    [SerializeField] public LayerMask interactLayerMask = ~0;
    [SerializeField] public float interactMaxDistance = 2.0f;

    [SyncVar] GameObject syncedFirstWeapon;
    [SyncVar] GameObject syncedSecondWeapon;
    [SyncVar] GameObject syncedGadget;
    [SyncVar] ItemSlot syncedEquipped = ItemSlot.PrimaryWeapon;

    GameObject localFirstWeapon;
    GameObject localSecondWeapon;
    GameObject localGadget;
    ItemSlot localEquipped = ItemSlot.PrimaryWeapon;

    static void UpdateEquippedItem(NetworkItem item, Transform anchor, bool holstered)
    {
        if (item == null)
            return;
        var transform = item.transform;
        transform.parent = anchor;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        EventBus.Trigger<bool>(nameof(EventItemPickedUp), item.gameObject, holstered);
    }

    public NetworkItem firstWeapon
    {
        get 
        {
            if (localFirstWeapon == null || localFirstWeapon.activeSelf == false)
                return null;
            return localFirstWeapon.GetComponent<NetworkItem>(); 
        }

        set
        {
            localFirstWeapon = value?.gameObject;
            if (this.isServer)
                syncedFirstWeapon = localFirstWeapon;
            UpdateEquippedItem(value, primaryWeaponAnchor, equipped != ItemSlot.PrimaryWeapon);
        }
    }
    public NetworkItem secondWeapon
    {
        get
        {
            if (localSecondWeapon == null || localSecondWeapon.activeSelf == false)
                return null;
            return localSecondWeapon.GetComponent<NetworkItem>();
        }
        set
        {
            localSecondWeapon = value?.gameObject;
            if (this.isServer)
                syncedSecondWeapon = localSecondWeapon;
            UpdateEquippedItem(value, secondaryWeaponAnchor, equipped != ItemSlot.SecondaryWeapon);
        }
    }
    public NetworkItem gadget
    {
        get {
            if (localGadget == null || localGadget.activeSelf == false)
                return null;
            return localGadget.GetComponent<NetworkItem>(); 
        }
        set
        {
            localGadget = value?.gameObject;
            if (this.isServer)
                syncedGadget = localSecondWeapon;
            UpdateEquippedItem(value, gadgetAnchor, equipped != ItemSlot.Gadget);
        }
    }

    public ItemSlot equipped
    {
        get { return localEquipped; }
        set
        {
            localEquipped = value;
            if (this.isServer)
            {
                syncedEquipped = value;
            }
        }
    }

    public NetworkItem GetItem(ItemSlot slot)
    {
        return slot switch
        {
            ItemSlot.PrimaryWeapon => firstWeapon,
            ItemSlot.SecondaryWeapon => secondWeapon,
            ItemSlot.Gadget => gadget,
            _ => null,
        };
    }

    public T GetItemComponent<T>(ItemSlot slot) where T : class
    {
        var item = GetItem(slot);
        if(item != null)
        {
            return item.GetComponent<T>();
        }
        return null;
    }

    public Transform GetSlotAnchor(ItemSlot slot)
    {
        return slot switch
        {
            ItemSlot.PrimaryWeapon => primaryWeaponAnchor != null ? primaryWeaponAnchor : this.transform,
            ItemSlot.SecondaryWeapon => secondaryWeaponAnchor != null ? secondaryWeaponAnchor : this.transform,
            ItemSlot.Gadget => gadgetAnchor != null ? gadgetAnchor : this.transform,
            _ => this.transform,
        };
    }

    static NetworkItem CastItem(GameObject go)
    {
        if(go != null && go.activeSelf)
        {
            return go.GetComponent<NetworkItem>();
        }
        return null;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        equipped = syncedEquipped;
        firstWeapon = CastItem(syncedFirstWeapon);
        secondWeapon = CastItem(syncedSecondWeapon);
        gadget = CastItem(syncedGadget);
    }


    void DoDropItem(ItemSlot slot)
    {
        var item = GetItem(slot);
        if (item == null)
            return;

        EventBus.Trigger(nameof(EventItemDropped), item.gameObject);
        if(item.hasAuthority)
        {
            item.CmdDropOwnership();
        }
    }

    #region Equip
    System.Collections.IEnumerator DoEquip(ItemSlot slot)
    {
        if (equipped != slot)
        {
            var previouslyEquipped = GetItem(equipped);
            
            equipped = slot;

            if (previouslyEquipped != null)
            {
                EventBus.Trigger(nameof(EventItemHolstered), previouslyEquipped.gameObject);
                while(previouslyEquipped.Activated)
                {
                    yield return null;
                }
            }
            
            var item = GetItem(equipped);
            if (item != null)
            {
                EventBus.Trigger(nameof(EventItemUnholstered), item.gameObject);
            }
        }

    }

    [ClientRpc(includeOwner = false)]
    void RpcEquip(ItemSlot slot)
    {
        StartCoroutine(DoEquip(slot));
    }

    [Command]
    void CmdEquip(ItemSlot slot)
    {
        syncedEquipped = slot;
        RpcEquip(slot);
    }

    public void Equip(ItemSlot slot)
    {
        if (this.netIdentity.HasControl())
        {
            StartCoroutine(DoEquip(slot));
            CmdEquip(slot);
        }
    }
    #endregion

    // Picks up and replaces the item slot
    void DoPickUpItem(NetworkItem go, ItemSlot slot)
    {
        DoDropItem(slot);

        switch(slot)
        {
            case ItemSlot.PrimaryWeapon:
                firstWeapon = go;
                break;
            case ItemSlot.SecondaryWeapon:
                secondWeapon = go;
                break;
            case ItemSlot.Gadget:
                gadget = go;
                break;
        }

        if (equipped != slot && isLocalPlayer && Settings.autoEquipOnPickup)
        {
            Equip(slot);
        }
    }

    void Update()
    {
        if (!isLocalPlayer)
            return;
        FindObjectOfType<PlayerGUI>().updateGUI(this);
    }


    public void UseActiveEquipment(bool primaryAttack, bool wasPressed)
    {
        var item = GetItemComponent<NetworkItem>(equipped);
        if (item == null)
        {
            return;
        }

        if (primaryAttack)
        {
            item.OnPrimaryAttack(wasPressed);
        }
        else
        {
            item.OnSecondaryAttack(wasPressed);
        }
    }

    public void NextItem()
    {
        if (equipped == ItemSlot.Gadget)
        {
            if (firstWeapon != null)
                Equip(ItemSlot.PrimaryWeapon);
            else if (secondWeapon != null)
                Equip(ItemSlot.SecondaryWeapon);
        }
        else if (equipped == ItemSlot.PrimaryWeapon)
        {
            if (secondWeapon != null)
                Equip(ItemSlot.SecondaryWeapon);
            else if (gadget != null)
                Equip(ItemSlot.Gadget);
        }
        else if (equipped == ItemSlot.SecondaryWeapon)
        {
            if (gadget != null)
                Equip(ItemSlot.Gadget);
            else if (firstWeapon != null)
                Equip(ItemSlot.PrimaryWeapon);
        }
    }

    void INetworkItemOwner.OnDestroyed(NetworkItem item)
    {
        if(this.hasAuthority && GetItemComponent<NetworkItem>(equipped) == item)
        {
            NextItem();
        }
    }

    void INetworkItemOwner.OnPickedUp(NetworkItem item)
    {
        if (item.ItemType == ItemType.Gadget)
        {
            DoPickUpItem(item, ItemSlot.Gadget);
        }
        else if (item.ItemType == ItemType.Weapon)
        {
            if (firstWeapon == null || ((secondWeapon != null) && equipped == ItemSlot.PrimaryWeapon))
            {
                // Pick up when first slot is empty or both slots are full and the active slot is the primary weapon
                DoPickUpItem(item, ItemSlot.PrimaryWeapon);
            }
            else
            {
                DoPickUpItem(item, ItemSlot.SecondaryWeapon);
            }
        }
        else
        {
            Debug.LogError("Unknown item picked up.");
        }
    }

    bool INetworkItemOwner.CanPickup(NetworkItem item)
    {
        if (item.ItemType == ItemType.Gadget)
        {
            return gadget == null || equipped == ItemSlot.Gadget;
        }
        else if(item.ItemType == ItemType.Weapon)
        {
            return firstWeapon == null || secondWeapon == null || equipped == ItemSlot.PrimaryWeapon || equipped == ItemSlot.SecondaryWeapon;
        }
        return false;
    }
}

[System.Serializable]
public enum ItemSlot
{
    PrimaryWeapon,
    SecondaryWeapon,
    Gadget
}