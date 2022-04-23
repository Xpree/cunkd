using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class Inventory : NetworkBehaviour, INetworkItemOwner
{
    [SerializeField] public Transform primaryWeaponAnchor;
    [SerializeField] public Transform secondaryWeaponAnchor;
    [SerializeField] public Transform gadgetAnchor;
    [SerializeField] public LayerMask interactLayerMask = ~0;
    [SerializeField] public float interactMaxDistance = 2.0f;

    [Header("Diagnostics")]
    public GameObject gadget;
    public GameObject firstWeapon;
    public GameObject secondWeapon;
    public ItemSlot equipped = ItemSlot.PrimaryWeapon;

    public IEquipable GetEquipment(ItemSlot slot)
    {
        return slot switch
        {
            ItemSlot.PrimaryWeapon => firstWeapon != null ? firstWeapon.GetComponent<IEquipable>() : null,
            ItemSlot.SecondaryWeapon => secondWeapon != null ? secondWeapon.GetComponent<IEquipable>() : null,
            ItemSlot.Gadget => gadget != null ? gadget.GetComponent<IEquipable>() : null,
            _ => null,
        };
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

    void DoDropItem(GameObject go)
    {
        if (go == null)
            return;

        go.GetComponent<IEquipable>()?.OnDropped();
        if (isLocalPlayer)
        {
            var item = go.GetComponent<NetworkItem>();
            if (item != null)
            {
                item.CmdDrop();
            }
        }
    }

    #region Remove slot
    GameObject DoRemoveSlot(ItemSlot slot)
    {
        GetEquipment(slot)?.OnRemoved();
        GameObject removed = null;
        switch (slot)
        {
            case ItemSlot.PrimaryWeapon:
                removed = firstWeapon;
                firstWeapon = null;
                break;
            case ItemSlot.SecondaryWeapon:
                removed = secondWeapon;
                secondWeapon = null;
                break;
            case ItemSlot.Gadget:
                removed = gadget;
                gadget = null;
                break;
        }
        return removed;
    }

    [ClientRpc(includeOwner = false)]
    void RpcRemoveSlot(ItemSlot slot)
    {
        if (isClientOnly)
        {
            DoRemoveSlot(slot);
        }
    }

    [Command]
    void CmdRemoveSlot(ItemSlot slot)
    {
        RpcRemoveSlot(slot);
        var go = DoRemoveSlot(slot);
        if (go != null)
            NetworkServer.Destroy(go);
    }

    public void RemoveSlot(ItemSlot slot)
    {
        if (this.netIdentity.HasControl())
        {
            if (isClientOnly)
                DoRemoveSlot(slot);
            CmdRemoveSlot(slot);
        }
    }
    #endregion

    #region Equip
    System.Collections.IEnumerator DoEquip(ItemSlot slot)
    {
        if (equipped != slot)
        {
            ActiveEquipment?.OnHolstered();
            while (!IsActiveEquipmentHolstered)
                yield return null;

            equipped = slot;
            ActiveEquipment?.OnUnholstered();
        }

    }

    [ClientRpc(includeOwner = false)]
    void RpcEquip(ItemSlot slot)
    {
        if (isClientOnly)
        {
            StartCoroutine(DoEquip(slot));
        }
    }

    [Command]
    void CmdEquip(ItemSlot slot)
    {
        RpcEquip(slot);
        StartCoroutine(DoEquip(slot));
    }

    public void Equip(ItemSlot slot)
    {
        if (this.netIdentity.HasControl())
        {
            if (isClientOnly)
                StartCoroutine(DoEquip(slot));
            CmdEquip(slot);
        }
    }
    #endregion

    // Picks up or replaces 'slotGo' with 'go'
    void DoPickUpItem(ref GameObject slotGo, GameObject go, ItemSlot slot)
    {
        DoDropItem(slotGo);
        slotGo = go;
        var child = go.transform;
        child.parent = GetSlotAnchor(slot);
        child.localPosition = Vector3.zero;
        child.localRotation = Quaternion.identity;

        var holstered = equipped != slot;
        go.GetComponent<IEquipable>()?.OnPickedUp(holstered);
        if (holstered && isLocalPlayer && Settings.autoEquipOnPickup)
        {
            Equip(slot);
        }
    }

    void Update()
    {
        if (!isLocalPlayer)
            return;

        HandleInput();
    }

    public void Interact()
    {
        var transform = Util.GetPlayerInteractAimTransform(this.gameObject);
        if (transform == null)
            return;

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, interactMaxDistance, this.interactLayerMask))
        {
            hit.transform.GetComponent<IInteractable>()?.Interact(this.netIdentity);
        }
    }

    public void UseActiveEquipment(bool primaryAttack, bool wasPressed)
    {
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

            if (activeGadget.ChargesLeft <= 0)
            {
                RemoveSlot(ItemSlot.Gadget);
            }
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
            if (firstWeapon != null)
                Equip(ItemSlot.PrimaryWeapon);
            else if (secondWeapon != null)
                Equip(ItemSlot.SecondaryWeapon);
        }
        else if (equipped == ItemSlot.PrimaryWeapon)
        {
            if (gadget != null)
                Equip(ItemSlot.Gadget);
            else if (secondWeapon != null)
                Equip(ItemSlot.SecondaryWeapon);
        }
        else if (equipped == ItemSlot.SecondaryWeapon)
        {
            if (gadget != null)
                Equip(ItemSlot.Gadget);
            else if (firstWeapon != null)
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

    void INetworkItemOwner.OnDropped(NetworkItem item)
    {
    }

    void INetworkItemOwner.OnPickedUp(NetworkItem item)
    {
        if (item.GetComponent<IWeapon>() != null)
        {
            if (firstWeapon == null || ((secondWeapon != null) && equipped == ItemSlot.PrimaryWeapon))
            {
                // Pick up when first slot is empty or both slots are full and the active slot is the primary weapon
                DoPickUpItem(ref firstWeapon, item.gameObject, ItemSlot.PrimaryWeapon);
            }
            else
            {
                DoPickUpItem(ref secondWeapon, item.gameObject, ItemSlot.SecondaryWeapon);
            }
        }
        else if (item.GetComponent<IGadget>() != null)
        {
            DoPickUpItem(ref gadget, item.gameObject, ItemSlot.Gadget);
        }
        else
        {
            Debug.LogError("Unknown item picked up.");
        }
    }

    bool INetworkItemOwner.CanPickup(NetworkItem item)
    {

        if (item.GetComponent<IEquipable>() != null)
        {
            if (item.GetComponent<IWeapon>() != null)
            {
                return firstWeapon == null || secondWeapon == null || equipped == ItemSlot.PrimaryWeapon || equipped == ItemSlot.SecondaryWeapon;
            }
            else if (item.GetComponent<IGadget>() != null)
            {
                return gadget == null || equipped == ItemSlot.Gadget;
            }

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
    void OnRemoved();
}