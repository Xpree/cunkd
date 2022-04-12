using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

public class Inventory : NetworkBehaviour
{
    IWeapon currentWeapon;
    [SyncVar(hook = nameof(onGadgetAdded))]GameObject gadget;
    
    private void Awake()
    {
        currentWeapon = GetComponent<BlackHoleGun>();
    }

    [Server]
    public void addGadget(GameObject gad)
    {
        GameObject go = Instantiate(gad, transform.position, Quaternion.identity);
        NetworkServer.Spawn(go);
        gadget = go;
    }

    [Client]
    private void onGadgetAdded(GameObject oldGameObject, GameObject newGameObject)
    {
        newGameObject.transform.SetParent(gameObject.transform);
        newGameObject.GetComponent<MeshRenderer>().enabled = false;
        if (isLocalPlayer)
        {
            print(newGameObject.name + " added to inventory");
        }
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

        if (Keyboard.current[Key.F1].wasPressedThisFrame)
        {
            if (gadget)
            {
                gadget.GetComponent<IGadget>().PrimaryUse(true);
            }
            else
            {
                print("no gadget avaiable");
            }
        }

        if (Keyboard.current[Key.F2].wasPressedThisFrame)
        {
            if (gadget)
            {
                gadget.GetComponent<IGadget>().SecondaryUse(true);
            }
            else
            {
                print("no gadget avaiable");
            }
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
