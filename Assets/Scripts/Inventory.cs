using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

public class Inventory : NetworkBehaviour
{
    IWeapon currentWeapon;
    
    private void Awake()
    {
        currentWeapon = GetComponent<GravityGun>();
    }

    // Temporary location of attack input
    [ClientCallback]
    void Update()
    {
        if (!isLocalPlayer)
            return;
        
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            currentWeapon.PrimaryAttack(true);
        }
        
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            currentWeapon.PrimaryAttack(false);
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            currentWeapon.SecondaryAttack(true);
        }
        
        if (Mouse.current.rightButton.wasReleasedThisFrame)
        {
            currentWeapon.SecondaryAttack(false);
        }
    }


    static void GUIDrawProgress(float progress)
    {
        if(progress > 0.0)
        {
            GUI.Box(new Rect(Screen.width * 0.5f - 50, Screen.height * 0.8f - 10, 100.0f * progress, 20.0f), GUIContent.none);
        }        
    }

    private void OnGUI()
    {
        if (!isLocalPlayer)
            return;

        if (currentWeapon.ChargeProgress is float progress)
        {
            GUIDrawProgress(progress);
        }
    }
}
