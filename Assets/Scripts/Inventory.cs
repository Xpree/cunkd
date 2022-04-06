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

    [ClientCallback]
    void Update()
    {

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
}
