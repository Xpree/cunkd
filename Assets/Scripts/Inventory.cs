using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

public class Inventory : NetworkBehaviour
{
    [SyncVar(hook = nameof(onGadgetAdded))]public GameObject gadget;
    [SyncVar(hook = nameof(onWeaponAdded))] GameObject firstWeapon;
    [SyncVar(hook = nameof(onWeaponAdded))] GameObject secondWeapon;
    [SyncVar] public GameObject currentWeapon;

    [SerializeField] GameObject startWeapon;
    [SerializeField]public Transform objectAnchor;
    [SerializeField] public Transform weaponAnchor;

    bool addWeaponsAtStart = true;

    [Server]
    public override void OnStartServer()
    {
        base.OnStartServer();
        GameObject weapon = Instantiate(startWeapon, transform.position, Quaternion.identity);
        NetworkServer.Spawn(weapon, this.connectionToClient);
        addWeapon(weapon);
    }

    [Server]
    public void addGadget(GameObject gad)
    {
        gadget = gad;
    }

    [Server]
    public void addWeapon(GameObject weapon)
    {
        print(weapon.name + "added to inventory");

        if (currentWeapon == firstWeapon)
        {
            firstWeapon = weapon;
            currentWeapon = weapon;
        }
        else
        {
            secondWeapon = weapon;
            currentWeapon = weapon;
        }
        if (addWeaponsAtStart)
        {
            GameObject weapon2 = Instantiate(startWeapon, transform.position, Quaternion.identity);
            NetworkServer.Spawn(weapon2, this.connectionToClient);
            addWeaponsAtStart = false;
            secondWeapon = weapon2;
        }
    }

    [Client]
    private void onGadgetAdded(GameObject oldGameObject, GameObject newGameObject)
    {
        if (isLocalPlayer)
        {
            print(newGameObject.name + " added to inventory");
        }
    }

    [Client]
    private void onWeaponAdded(GameObject oldGameObject, GameObject newGameObject)
    {
        newGameObject.GetComponent<IWeapon>().initializeOnPlayer(this);

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

        if (Keyboard.current[Key.Q].wasPressedThisFrame)
        {
            CmdSwapWeapon();
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            currentWeapon.GetComponent<IWeapon>().PrimaryAttack(true);
        }
        
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            currentWeapon.GetComponent<IWeapon>().PrimaryAttack(false);
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            currentWeapon.GetComponent<IWeapon>().SecondaryAttack(true);
        }
        
        if (Mouse.current.rightButton.wasReleasedThisFrame)
        {
            currentWeapon.GetComponent<IWeapon>().SecondaryAttack(false);
        }

        if (Keyboard.current[Key.F1].wasPressedThisFrame)
        {
            if (gadget)
            {
                gadget.GetComponent<IGadget>().PrimaryUse(true);
                if (gadget.GetComponent<IGadget>().ChargesLeft <= 0)
                {
                    CmdRemoveGadget();
                }
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
                if (gadget.GetComponent<IGadget>().ChargesLeft <= 0)
                {
                    CmdRemoveGadget();
                }
            }
            else
            {
                print("no gadget avaiable");
            }
        }

        //update weapon transform on client
        if (currentWeapon)
        {
            currentWeapon.transform.position = weaponAnchor.transform.position;
            currentWeapon.transform.rotation = weaponAnchor.transform.rotation;
        }
        if (gadget)
        {
            gadget.transform.position = weaponAnchor.transform.position;
            gadget.transform.rotation = weaponAnchor.transform.rotation;
        }

        //update weapon stransform on server
        CmdUpdateTransforms();
    }

    [Command]
    void CmdRemoveGadget()
    {
        NetworkServer.Destroy(gadget);
    }

    [Command]
    void CmdUpdateTransforms()
    {
        if (currentWeapon)
        {
            currentWeapon.transform.position = weaponAnchor.transform.position;
            currentWeapon.transform.rotation = weaponAnchor.transform.rotation;
        }
        if (gadget)
        {
            gadget.transform.position = weaponAnchor.transform.position;
            gadget.transform.rotation = weaponAnchor.transform.rotation;
        }
    }

    [Command]
    void CmdSwapWeapon()
    {
        currentWeapon.transform.localScale = new Vector3(0, 0, 0);
        if (currentWeapon == firstWeapon)
        {
            currentWeapon = secondWeapon;
        }
        else if (secondWeapon)
        {
            currentWeapon = firstWeapon;
        }
        currentWeapon.transform.localScale = new Vector3(0.07f, 0.07f, 0.07f);
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

        if (currentWeapon != null && currentWeapon.GetComponent<IWeapon>().ChargeProgress is float progress)
        {
            GUIDrawProgress(progress);
        }
    }
}
