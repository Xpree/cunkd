using UnityEngine.UI;
using UnityEngine;
using UnityEditor;
using TMPro;
using Mirror;

public class PlayerGUI : MonoBehaviour
{
    [SerializeField] GameObject interactUI;
    [SerializeField] TextMeshProUGUI intreractableInfoText;
    [SerializeField] TextMeshProUGUI cooldownText;

    [SerializeField] RawImage primaryWeaponIcon;
    [SerializeField] RawImage cooldownIconSlot1;
    [SerializeField] TextMeshProUGUI chargesSlot1;

    [SerializeField] RawImage secondaryWeaponIcon;
    [SerializeField] RawImage cooldownIconSlot2;
    [SerializeField] TextMeshProUGUI chargesSlot2;

    [SerializeField] RawImage gadgetIcon;
    [SerializeField] RawImage cooldownIconSlot3;
    [SerializeField] TextMeshProUGUI chargesSlot3;

    [SerializeField] RawImage selectedIcon;
    [SerializeField] Inventory inventory;

    [SerializeField] UILives localLives;

    [SerializeField] TMPro.TMP_Text hoverText;
    
    public void SetInteraction(string text)
    {
        intreractableInfoText.text = text;
        interactUI.SetActive(true);
    }

    public void HideInteraction()
    {
        interactUI.SetActive(false);
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

    void updateGUI()
    {
        selectedIcon.enabled = false;
        updateItem(inventory.GetItemComponent<NetworkItem>(ItemSlot.PrimaryWeapon), primaryWeaponIcon, cooldownIconSlot1, chargesSlot1, inventory.equipped == ItemSlot.PrimaryWeapon);
        updateItem(inventory.GetItemComponent<NetworkItem>(ItemSlot.SecondaryWeapon), secondaryWeaponIcon, cooldownIconSlot2, chargesSlot2, inventory.equipped == ItemSlot.SecondaryWeapon);
        updateItem(inventory.GetItemComponent<NetworkItem>(ItemSlot.Gadget), gadgetIcon, cooldownIconSlot3, chargesSlot3, inventory.equipped == ItemSlot.Gadget);
    }

    public void SetLocalLives(int lives)
    {
        if (lives > 0)
        {
            localLives.gameObject.SetActive(true);
            localLives.SetLives(lives);
        }
        else
        {
            localLives.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        updateGUI();
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
        if (inventory.ActiveWeapon?.ChargeProgress is float progress)
        {
            GUIDrawProgress(progress);
        }
    }

    
    public void SetHovertext(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            this.hoverText.gameObject.SetActive(false);
        }
        else
        {
            this.hoverText.text = text;
            this.hoverText.gameObject.SetActive(true);
        }
    }
}
