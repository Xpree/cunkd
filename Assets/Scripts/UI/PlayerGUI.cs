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
    private void Update()
    {
        //castRay();
        //updateGUI(inventory);
    }

    public void castRay()
    {
        var transform = Util.GetPlayerInteractAimTransform(inventory.gameObject);
        if (transform == null)
            return;

        ObjectSpawner obs = null;
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, inventory.interactMaxDistance, inventory.interactLayerMask))
        {
            obs = hit.transform.GetComponent<ObjectSpawner>();
            if (obs && !obs.IsPowerUpSpawner && obs.spawnedItem)
            {
                interactiveButton(obs);
            }
            else
            {
                interactiveButton(null);
            }
        }
        else
        {
            interactiveButton(null);
        }
    }

    public void interactiveButton(ObjectSpawner obs)
    {
        if (obs && !obs.IsPowerUpSpawner && obs.spawnedItem)
        {
            interactButton.enabled = true;
            intreractableInfoText.text = "Pick up " + obs.spawnedItem.name.Substring(0, obs.spawnedItem.name.Length - 7);
        }
        else
        {
            interactButton.enabled = false;
            intreractableInfoText.text = "";
        }
    }

    void setIcon(RawImage icon, GameObject go)
    {
        if (go)
        {
            icon.enabled = true;
            //icon.texture = AssetPreview.GetAssetPreview(go);
        }
        else
        {
            icon.enabled = false;
        }
    }

    public void assignCamera(Camera camera)
    {
        print("assigning camera");
        Canvas canvas = GetComponent<Canvas>();
        canvas.worldCamera = camera;
        canvas.planeDistance = 0.4f;
    }

    void updateCooldown(RawImage cooldownIcon, GameObject go)
    {
        NetworkCooldown cooldown = null;
        if (go)
            cooldown = go.GetComponent<NetworkCooldown>();
        if (cooldown && cooldown.HasCooldown)
        {
            cooldownIcon.rectTransform.localScale = new Vector3(1,Mathf.Clamp((float)-cooldown.localTimer.Elapsed / (float)cooldown.coolDownDuration * 1,0,1),1);
        }
        else if (cooldownIcon.rectTransform.localScale.y != 0)
        {
            cooldownIcon.rectTransform.localScale = new Vector3(0, 0, 0);
        }
    }

    void updateCharges(TextMeshProUGUI chargesText, GameObject go)
    {
        if (go)
        {
            var gadget = go.GetComponent<IGadget>();
            if (gadget != null)
            {
                chargesText.text = gadget.ChargesLeft + "/" + gadget.Charges;
            }
        }
        else
        {
            chargesText.text = "";
        }
    }

    public void updateGUI(Inventory inventory)
    {

        setIcon(primaryWeaponIcon, inventory.syncedFirstWeapon);
        setIcon(secondaryWeaponIcon, inventory.syncedSecondWeapon);
        setIcon(gadgetIcon, inventory.syncedGadget);

        updateCooldown(cooldownIconSlot1, inventory.syncedFirstWeapon);
        updateCooldown(cooldownIconSlot2, inventory.syncedSecondWeapon);
        updateCooldown(cooldownIconSlot3, inventory.syncedGadget);

        updateCharges(chargesSlot3, inventory.syncedGadget);

        selectedIcon.enabled = false;
        if (inventory.syncedEquipped == ItemSlot.PrimaryWeapon && inventory.syncedFirstWeapon)
        {
            selectedIcon.rectTransform.position = primaryWeaponIcon.rectTransform.position;
            selectedIcon.enabled = true;
        }
        else if (inventory.syncedEquipped == ItemSlot.SecondaryWeapon && inventory.syncedSecondWeapon)
        {
            selectedIcon.rectTransform.position = secondaryWeaponIcon.rectTransform.position;
            selectedIcon.enabled = true;
        }
        else if (inventory.syncedEquipped == ItemSlot.Gadget && inventory.syncedGadget)
        {
            selectedIcon.rectTransform.position = gadgetIcon.rectTransform.position;
            selectedIcon.enabled = true;
        }
    }
}
