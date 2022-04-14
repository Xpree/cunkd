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
        currentWeapon = GetComponent<GravityGun>();
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

        if (Keyboard.current[Key.E].wasPressedThisFrame)
        {
            shootRay();
        }

        if (Keyboard.current[Key.Digit1].wasPressedThisFrame)
        {
            if(currentWeapon is GravityGun)
            {
                currentWeapon = GetComponent<BlackHoleGun>();
            } 
            else
            {
                currentWeapon = GetComponent<GravityGun>();
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

        GUI.Box(new Rect(Screen.width * 0.5f - 1, Screen.height * 0.5f - 1, 2, 2), GUIContent.none);
        
        if (currentWeapon?.ChargeProgress is float progress)
        {
            GUIDrawProgress(progress);
        }
    }


    void shootRay()
    {
        RaycastHit hit;
        //print("shooting ray");
        Camera cam = gameObject.GetComponentInChildren<PlayerCameraController>().playerCamera;
        Ray ray = cam.ScreenPointToRay(new Vector2(Screen.width, Screen.height) / 2);
        if (Physics.Raycast(ray.origin, ray.direction, out hit, 15))
        {
            print("object hit: " + hit.transform.gameObject);

            ObjectSpawner objectSpawner = hit.transform.gameObject.GetComponent<ObjectSpawner>();
            if (objectSpawner)
            {
                CmdPickupObject(objectSpawner);
            }
        }
    }

    [Command]
    void CmdPickupObject(ObjectSpawner objectSpawner)
    {
        GameObject pickedUpObject = objectSpawner.pickupObject();
        Inventory inventory = gameObject.GetComponent<Inventory>();

        ScoreCard scorecard = gameObject.GetComponent<ScoreCard>();
        IGadget gadget = pickedUpObject.GetComponent<IGadget>();

        if (pickedUpObject.name == "Extra Life")
        {
            scorecard.livesLeft++;
        }
        if (gadget != null)
        {
            inventory.addGadget(pickedUpObject);
        }
    }

}
