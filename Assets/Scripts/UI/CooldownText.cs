using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CooldownText : MonoBehaviour
{
    public TextMeshProUGUI cooldownUI;
    public NetworkCooldown playerCooldown;

    private void Update() {
        playerCooldown = GetComponent<NetworkCooldown>();
        
        cooldownUI.text = "Cooldown: " + playerCooldown;    //playerCooldown needs to be something else here. Can't figure out what or
                                                            //where to get it from.
    }    
}