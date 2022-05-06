using UnityEngine.UI;
using UnityEngine;
using UnityEditor;
using TMPro;
using Mirror;

public class PlayerGUI : MonoBehaviour
{
    [SerializeField] public Image interactButton;
    [SerializeField] public TextMeshProUGUI intreractableInfoText;
    [SerializeField] public TextMeshProUGUI cooldownText;

    [SerializeField] public RawImage primaryWeaponIcon;
    [SerializeField] public RawImage cooldownIconSlot1;
    [SerializeField] public TextMeshProUGUI chargesSlot1;

    [SerializeField] public RawImage secondaryWeaponIcon;
    [SerializeField] public RawImage cooldownIconSlot2;
    [SerializeField] public TextMeshProUGUI chargesSlot2;

    [SerializeField] public RawImage gadgetIcon;
    [SerializeField] public RawImage cooldownIconSlot3;
    [SerializeField] public TextMeshProUGUI chargesSlot3;

    [SerializeField] public RawImage selectedIcon;
    [SerializeField] public Inventory inventory;


    //[Client]
    //private void Update()
    //{
    //    //castRay();
    //    //updateGUI(inventory);
    //}

    //public void castRay()
    //{
    //    var transform = Util.GetPlayerInteractAimTransform(inventory.gameObject);
    //    if (transform == null)
    //        return;

    //    ObjectSpawner obs = null;
    //    if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, inventory.interactMaxDistance, inventory.interactLayerMask))
    //    {
    //        obs = hit.transform.GetComponent<ObjectSpawner>();
    //        if (obs && obs.IsEquipmentSpawner && obs.spawnedItem)
    //        {
    //            interactiveButton(obs);
    //        }
    //        else
    //        {
    //            interactiveButton(null);
    //        }
    //    }
    //    else
    //    {
    //        interactiveButton(null);
    //    }
    //}

    //public void interactiveButton(ObjectSpawner obs)
    //{
    //    if (obs && obs.IsEquipmentSpawner && obs.spawnedItem)
    //    {
    //        interactButton.enabled = true;
    //        intreractableInfoText.text = "Pick up " + obs.spawnedItem.GetComponent<NetworkItem>().displayName;
    //    }
    //    else
    //    {
    //        interactButton.enabled = false;
    //        intreractableInfoText.text = "";
    //    }
    //}

    public void interactiveItemButton(NetworkItem obs)
    {
        if (obs != null)
        {
            interactButton.enabled = true;
            intreractableInfoText.text = "Pick up " + obs.displayName;
        }
        else
        {
            interactButton.enabled = false;
            intreractableInfoText.text = "";
        }
    }

    void setIcon(RawImage icon, NetworkItem item)
    {
        if (item != null)
        {
            icon.gameObject.SetActive(true);
            //icon.texture = AssetPreview.GetAssetPreview(item.gameObject);
        }
        else
        {
            icon.gameObject.SetActive(false);
        }
    }

    public void assignCamera(Camera camera)
    {
        print("assigning camera");
        Canvas canvas = GetComponent<Canvas>();
        canvas.worldCamera = camera;
        canvas.planeDistance = 0.4f;
    }

    void updateCooldown(RawImage cooldownIcon, NetworkCooldown cooldown)
    {
        if (cooldown && cooldown.HasCooldown)
        {
            float t = Mathf.Clamp01(cooldown.CooldownRemaining / cooldown.CooldownDuration);
            cooldownIcon.rectTransform.localScale = new Vector3(1,t,1);
        }
        else if (cooldownIcon.rectTransform.localScale.y != 0)
        {
            cooldownIcon.rectTransform.localScale = Vector3.zero;
        }
    }

    void updateCharges(TextMeshProUGUI chargesText, NetworkCooldown cooldown)
    {
        if (cooldown != null && cooldown.HasInfiniteCharges == false)
        {
            chargesText.text = cooldown.Charges + "/" + cooldown.MaxCharges;
        }
        else
        {
            chargesText.text = "";
        }
    }

    void updateItem(NetworkItem item, RawImage icon, RawImage cooldownIconSlot, TextMeshProUGUI chargesSlot, bool equipped)
    {
        if(item == null)
        {
            setIcon(icon, null);
            updateCooldown(cooldownIconSlot, null);
            if (chargesSlot != null)
                updateCharges(chargesSlot, null);
        } 
        else
        {
            setIcon(icon, item);
            var cooldown = item.GetComponent<NetworkCooldown>();
            updateCooldown(cooldownIconSlot, cooldown);
            if(chargesSlot != null)
                updateCharges(chargesSlot, cooldown);

            if (equipped)
            {
                selectedIcon.rectTransform.position = icon.rectTransform.position;
                selectedIcon.enabled = true;
            }
        }
    }

    public void updateGUI(Inventory inventory)
    {
        selectedIcon.enabled = false;
        updateItem(inventory.GetItemComponent<NetworkItem>(ItemSlot.PrimaryWeapon), primaryWeaponIcon, cooldownIconSlot1, chargesSlot1, inventory.equipped == ItemSlot.PrimaryWeapon);
        updateItem(inventory.GetItemComponent<NetworkItem>(ItemSlot.SecondaryWeapon), secondaryWeaponIcon, cooldownIconSlot2, chargesSlot2, inventory.equipped == ItemSlot.SecondaryWeapon);
        updateItem(inventory.GetItemComponent<NetworkItem>(ItemSlot.Gadget), gadgetIcon, cooldownIconSlot3, chargesSlot3, inventory.equipped == ItemSlot.Gadget);
    }
}
