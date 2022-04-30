using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using UnityEngine.UI;
using Unity.VisualScripting;

public class Inventory : NetworkBehaviour, INetworkItemOwner
{
    [SerializeField] public Transform primaryWeaponAnchor;
    [SerializeField] public Transform secondaryWeaponAnchor;
    [SerializeField] public Transform gadgetAnchor;
    [SerializeField] public LayerMask interactLayerMask = ~0;
    [SerializeField] public float interactMaxDistance = 2.0f;

    [Header("Diagnostics")]
    [SyncVar] public GameObject syncedFirstWeapon;
    [SyncVar] public GameObject syncedSecondWeapon;
    [SyncVar] public GameObject syncedGadget;
    [SyncVar] public ItemSlot syncedEquipped = ItemSlot.PrimaryWeapon;

    public GameObject localFirstWeapon;
    public GameObject localSecondWeapon;
    public GameObject localGadget;
    public ItemSlot localEquipped = ItemSlot.PrimaryWeapon;

    public GameObject firstWeapon
    {
        get {
            if (localFirstWeapon != null && localFirstWeapon.activeSelf == false)
                return null;
            return localFirstWeapon; 
        }
        set
        {
            localFirstWeapon = value;
            if (this.isServer)
                syncedFirstWeapon = value;
        }
    }
    public GameObject secondWeapon
    {
        get
        {
            if (localSecondWeapon != null && localSecondWeapon.activeSelf == false)
                return null;
            return localSecondWeapon;
        }
        set
        {
            localSecondWeapon = value;
            if (this.isServer)
                syncedSecondWeapon = value;
        }
    }
    public GameObject gadget
    {
        get {
            if (localGadget != null && localGadget.activeSelf == false)
                return null;
            return localGadget; 
        }
        set
        {
            localGadget = value;
            if (this.isServer)
                syncedGadget = value;
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

    public GameObject GetItem(ItemSlot slot)
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

    public IEquipable GetEquipment(ItemSlot slot)
    {
        return GetItemComponent<IEquipable>(slot);
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

    public IEquipable ActiveEquipment => GetEquipment(equipped);

    public bool IsActiveEquipmentHolstered => ActiveEquipment?.IsHolstered ?? true;
    public bool CanUseActiveEquipment => !IsActiveEquipmentHolstered;

    public IWeapon ActiveWeapon => equipped switch
    {
        ItemSlot.PrimaryWeapon => firstWeapon != null ? firstWeapon.GetComponent<IWeapon>() : null,
        ItemSlot.SecondaryWeapon => secondWeapon != null ? secondWeapon.GetComponent<IWeapon>() : null,
        _ => null,
    };

    public IGadget ActiveGadget => equipped switch
    {
        ItemSlot.Gadget => gadget != null ? gadget.GetComponent<IGadget>() : null,
        _ => null,
    };


    GameInputs gameInputs;
    private void Awake()
    {
        gameInputs = GetComponentInChildren<GameInputs>(true);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        localFirstWeapon = syncedFirstWeapon;
        localSecondWeapon = syncedSecondWeapon;
        localGadget = syncedGadget;
        localEquipped = syncedEquipped;
        UpdateEquippedItem(ItemSlot.PrimaryWeapon);
        UpdateEquippedItem(ItemSlot.SecondaryWeapon);
        UpdateEquippedItem(ItemSlot.Gadget);
    }


    static void AttachGo(GameObject child, Transform parent)
    {
        if (child == null)
        {
            return;
        }

        child.transform.parent = parent;
        child.transform.localPosition = Vector3.zero;
        child.transform.localRotation = Quaternion.identity;
    }

    static void UpdateEquippedItem(GameObject go, Transform anchor, bool holstered)
    {
        if (go == null)
        {
            return;
        }
        AttachGo(go, anchor);
        go.GetComponent<IEquipable>()?.OnPickedUp(holstered);
        EventBus.Trigger<bool>(nameof(EventItemPickedUp), go, holstered);
    }

    void UpdateEquippedItem(ItemSlot slot)
    {
        switch (slot)
        {
            case ItemSlot.PrimaryWeapon:
                UpdateEquippedItem(firstWeapon, primaryWeaponAnchor, slot != equipped);
                break;
            case ItemSlot.SecondaryWeapon:
                UpdateEquippedItem(secondWeapon, secondaryWeaponAnchor, slot != equipped);
                break;
            case ItemSlot.Gadget:
                UpdateEquippedItem(gadget, gadgetAnchor, slot != equipped);
                break;
        }
    }

    void DoDropItem(GameObject go)
    {
        if (go == null)
            return;

        go.GetComponent<IEquipable>()?.OnDropped();
        EventBus.Trigger(nameof(EventItemDropped), go);
        var item = go.GetComponent<NetworkItem>();
        if (item != null && item.hasAuthority)
        {
            item.CmdDropOwnership();
        }
    }

    #region Equip
    System.Collections.IEnumerator DoEquip(ItemSlot slot)
    {
        if (equipped != slot)
        {
            var item = GetItem(equipped);
            if (item != null)
            {
                EventBus.Trigger(nameof(EventItemHolstered), item);
            }

            var active = GetEquipment(equipped);
            if (active != null)
            {
                active.OnHolstered();
                while (active.IsHolstered == false)
                    yield return null;
            }
            equipped = slot;
            active = GetEquipment(slot);
            if (active != null)
            {
                active.OnUnholstered();
            }
            
            item = GetItem(equipped);
            if (item != null)
            {
                EventBus.Trigger(nameof(EventItemUnholstered), item);
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
        if (this.isServerOnly)
            StartCoroutine(DoEquip(slot));
    }

    public void Equip(ItemSlot slot)
    {
        StartCoroutine(DoEquip(slot));
        if (this.netIdentity.HasControl())
        {
            CmdEquip(slot);
        }
    }
    #endregion

    // Picks up and replaces the item slot
    void DoPickUpItem(GameObject go, ItemSlot slot)
    {
        switch(slot)
        {
            case ItemSlot.PrimaryWeapon:
                DoDropItem(firstWeapon);
                firstWeapon = go;
                break;
            case ItemSlot.SecondaryWeapon:
                DoDropItem(secondWeapon);
                secondWeapon = go;
                break;
            case ItemSlot.Gadget:
                DoDropItem(gadget);
                gadget = go;
                break;
        }
        AttachGo(go, GetSlotAnchor(slot));
        var holstered = equipped != slot;

        go.GetComponent<IEquipable>()?.OnPickedUp(holstered);
        EventBus.Trigger<bool>(nameof(EventItemPickedUp), go, holstered);
        if (holstered && isLocalPlayer && Settings.autoEquipOnPickup)
        {
            Equip(slot);
        }
    }

    void Update()
    {
        if (!isLocalPlayer)
            return;
        castRay();
        HandleInput();
        FindObjectOfType<PlayerGUI>().updateGUI(this);
    }

    public void castRay()
    {
        var transform = Util.GetPlayerInteractAimTransform(this.gameObject);
        if (transform == null)
            return;

        PlayerGUI gui = FindObjectOfType<PlayerGUI>();
        ObjectSpawner obs = null;
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, interactMaxDistance, interactLayerMask))
        {
            obs = hit.transform.GetComponent<ObjectSpawner>();
        }
        gui.interactiveButton(obs);
    }

    public void Interact()
    {
        var transform = Util.GetPlayerInteractAimTransform(this.gameObject);
        if (transform == null)
            return;

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, interactMaxDistance, this.interactLayerMask))
        {
            EventBus.Trigger(nameof(EventPlayerInteract), hit.collider.gameObject, this.netIdentity);
            hit.transform.GetComponent<IInteractable>()?.Interact(this.netIdentity);
        }
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


        if (!CanUseActiveEquipment)
            return;


        if (equipped == ItemSlot.Gadget)
        {
            var activeGadget = ActiveGadget;
            if (activeGadget == null)
                return;


            if (primaryAttack)
                activeGadget.PrimaryUse(wasPressed);
            else
                activeGadget.SecondaryUse(wasPressed);
        }
        else
        {
            if (primaryAttack)
                ActiveWeapon?.PrimaryAttack(wasPressed);
            else
                ActiveWeapon?.SecondaryAttack(wasPressed);
        }
    }

    public void NextItem()
    {
        if (equipped == ItemSlot.Gadget)
        {
            if (firstWeapon != null && firstWeapon.activeSelf)
                Equip(ItemSlot.PrimaryWeapon);
            else if (secondWeapon != null && secondWeapon.activeSelf)
                Equip(ItemSlot.SecondaryWeapon);
        }
        else if (equipped == ItemSlot.PrimaryWeapon)
        {
            if (secondWeapon != null && secondWeapon.activeSelf)
                Equip(ItemSlot.SecondaryWeapon);
            else if (gadget != null && gadget.activeSelf)
                Equip(ItemSlot.Gadget);
        }
        else if (equipped == ItemSlot.SecondaryWeapon)
        {
            if (gadget != null && gadget.activeSelf)
                Equip(ItemSlot.Gadget);
            else if (firstWeapon != null && firstWeapon.activeSelf)
                Equip(ItemSlot.PrimaryWeapon);
        }
    }



    [Client]
    void HandleInput()
    {
        if (gameInputs.NextItem.triggered)
        {
            NextItem();
        }

        if (gameInputs.SelectItem1.triggered)
        {
            Equip(ItemSlot.PrimaryWeapon);
        }

        if (gameInputs.SelectItem2.triggered)
        {
            Equip(ItemSlot.SecondaryWeapon);
        }

        if (gameInputs.SelectItem3.triggered)
        {
            Equip(ItemSlot.Gadget);
        }

        if (gameInputs.PrimaryAttack.WasPressedThisFrame() || gameInputs.PrimaryAttack.WasReleasedThisFrame())
        {
            var wasPressed = gameInputs.PrimaryAttack.WasPressedThisFrame();
            UseActiveEquipment(true, wasPressed);
        }

        //if (gameInputs.SecondaryAttack.IsPressed())
        //{
        //    var wasPressed = gameInputs.SecondaryAttack.IsPressed();
        //    UseActiveEquipment(false, wasPressed);
        //}

        if (gameInputs.SecondaryAttack.WasPressedThisFrame() || gameInputs.SecondaryAttack.WasReleasedThisFrame())
        {
            var wasPressed = gameInputs.SecondaryAttack.WasPressedThisFrame();
            UseActiveEquipment(false, wasPressed);
        }

        if (gameInputs.Interact.triggered)
        {
            Interact();
        }
    }


    static void GUIDrawProgress(float progress)
    {
        if (progress > 0.0)
        {
            GUI.Box(new Rect(Screen.width * 0.5f - 50, Screen.height * 0.8f - 10, 100.0f * progress, 20.0f), GUIContent.none);
        }
    }

    private void OnGUI()
    {
        if (!isLocalPlayer)
            return;

        GUI.Box(new Rect(Screen.width * 0.5f - 1, Screen.height * 0.5f - 1, 2, 2), GUIContent.none);

        if (ActiveWeapon?.ChargeProgress is float progress)
        {
            GUIDrawProgress(progress);
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
        if (item.GetComponent<IGadget>() != null || item.ItemType == ItemType.Gadget)
        {
            DoPickUpItem(item.gameObject, ItemSlot.Gadget);
        }
        else if (item.GetComponent<IWeapon>() != null || item.ItemType == ItemType.Weapon)
        {
            if (firstWeapon == null || ((secondWeapon != null) && equipped == ItemSlot.PrimaryWeapon))
            {
                // Pick up when first slot is empty or both slots are full and the active slot is the primary weapon
                DoPickUpItem(item.gameObject, ItemSlot.PrimaryWeapon);
            }
            else
            {
                DoPickUpItem(item.gameObject, ItemSlot.SecondaryWeapon);
            }
        }
        else
        {
            Debug.LogError("Unknown item picked up.");
        }
    }

    bool INetworkItemOwner.CanPickup(NetworkItem item)
    {
        if (item.GetComponent<IGadget>() != null || item.ItemType == ItemType.Gadget)
        {
            return gadget == null || equipped == ItemSlot.Gadget;
        }
        else if (item.GetComponent<IWeapon>() != null || item.ItemType == ItemType.Weapon)
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

public interface IInteractable
{
    /// <summary>
    /// Invoked on successful interact raycast by Inventory. Identity is the player
    /// </summary>
    void Interact(NetworkIdentity identity);
}

// Callback functions from Inventory component
public interface IEquipable
{
    bool IsHolstered { get; }
    void OnHolstered();
    void OnUnholstered();
    void OnPickedUp(bool startHolstered);
    void OnDropped();
}
