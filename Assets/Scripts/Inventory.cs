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

    GameInputs gameInputs;

    [Server]
    public override void OnStartServer()
    {
        base.OnStartServer();
        GameObject weapon = Instantiate(startWeapon, transform.position, Quaternion.identity);
        NetworkServer.Spawn(weapon, this.connectionToClient);
        addWeapon(weapon);
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        gameInputs = FindObjectOfType<GameInputs>();
    }

    [Server]
    public void addGadget(GameObject gad)
    {
        gadget = gad;
        if (equipped != 3)
        {
            gad.transform.localScale = new Vector3(0, 0, 0);
        }
    }

    [Server]
    public void addWeapon(GameObject weapon)
    {
        if (equipped == 3)
        {
            weapon.transform.localScale = new Vector3(0, 0, 0);
        }
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
            firstWeapon = weapon;
            currentWeapon = weapon;
            GameObject weapon2 = Instantiate(startWeapon, transform.position, Quaternion.identity);
            NetworkServer.Spawn(weapon2, this.connectionToClient);
            addWeaponsAtStart = false;
            secondWeapon = weapon2;
            weapon2.transform.localScale = new Vector3(0, 0, 0);
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

    [Client]
    void HandleInput()
    {

        if (gameInputs.NextItem.triggered)
        {
            NextInventoryItem();
        }

        if (gameInputs.SelectItem1.triggered)
        {
            CmdswapTo(1);
        }

        if (gameInputs.SelectItem2.triggered)
        {
            CmdswapTo(2);
        }

        if (gameInputs.SelectItem3.triggered)
        {
            CmdswapTo(3);
        }

        if (gameInputs.PrimaryAttack.WasPressedThisFrame() || gameInputs.PrimaryAttack.WasReleasedThisFrame())
        {
            var wasPressed = gameInputs.PrimaryAttack.WasPressedThisFrame();
            if (equipped == 3)
            {
                gadget.GetComponent<IGadget>().PrimaryUse(wasPressed);
                if (gadget.GetComponent<IGadget>().ChargesLeft <= 0)
                {
                    NextInventoryItem();
                    CmdRemoveGadget();
                }
            }
            else
                currentWeapon.GetComponent<IWeapon>().PrimaryAttack(wasPressed);
        }


        if (gameInputs.SecondaryAttack.WasPressedThisFrame() || gameInputs.SecondaryAttack.WasReleasedThisFrame())
        {
            var wasPressed = gameInputs.SecondaryAttack.WasPressedThisFrame();
            if (equipped == 3)
            {
                gadget.GetComponent<IGadget>().SecondaryUse(wasPressed);
                if (gadget.GetComponent<IGadget>().ChargesLeft <= 0)
                {
                    NextInventoryItem();
                    CmdRemoveGadget();
                }
            }
            else
                currentWeapon.GetComponent<IWeapon>().SecondaryAttack(wasPressed);
        }


        if (gameInputs.Interact.triggered)
        {
            Interact();
        }
    }

    [ClientCallback]
    void Update()
    {
        if (!isLocalPlayer)
            return;

        HandleInput();
        UpdateTransforms();

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

    [Client]
    void UpdateTransforms()
    {
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

    int equipped = 1;

    [Client]
    void NextInventoryItem()
    {
        if (2 < equipped)
            equipped = 0;
        if (!gadget && equipped == 2)
            equipped = 0;
        CmdswapTo(++equipped);
    }

    [Command]
    void CmdswapTo(int equip)
    {
        //weapon 1
        if (equip == 1)
        {
            currentWeapon = firstWeapon;
            if (gadget)
            {
                gadget.transform.localScale = new Vector3(0, 0, 0);
            }
            firstWeapon.transform.localScale = new Vector3(1, 1, 1);
            secondWeapon.transform.localScale = new Vector3(0, 0, 0);
        }
        //weapon 2
        if (equip == 2)
        {
            currentWeapon = secondWeapon;
            if (gadget)
            {
                gadget.transform.localScale = new Vector3(0, 0, 0);
            }
            firstWeapon.transform.localScale = new Vector3(0, 0, 0);
            secondWeapon.transform.localScale = new Vector3(1, 1, 1);
        }
        //gadget
        if (equip == 3 && gadget)
        {
            gadget.transform.localScale = new Vector3(1, 1, 1);
            firstWeapon.transform.localScale = new Vector3(0, 0, 0);
            secondWeapon.transform.localScale = new Vector3(0, 0, 0);
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

        if (currentWeapon.GetComponent<IWeapon>()?.ChargeProgress is float progress)
        {
            GUIDrawProgress(progress);
        }
    }


    void Interact()
    {
        //print("shooting ray");
        RaycastHit hit;
        Camera cam = gameObject.GetComponentInChildren<PlayerCameraController>().playerCamera;
        Ray ray = cam.ScreenPointToRay(new Vector2(Screen.width, Screen.height) / 2);
        if (Physics.Raycast(ray.origin, ray.direction, out hit, 15))
        {
            //print("object hit: " + hit.transform.gameObject);

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
        Inventory inventory = gameObject.GetComponent<Inventory>();
        GameObject pickedUpObject = objectSpawner.pickupObject(this);

        if (pickedUpObject)
        {
            ScoreCard scorecard = gameObject.GetComponent<ScoreCard>();
            IGadget gadget = pickedUpObject.GetComponent<IGadget>();
            IWeapon weapon = pickedUpObject.GetComponent<IWeapon>();

            if (pickedUpObject.name == "Extra Life")
            {
                scorecard.livesLeft++;
            }
            if (gadget != null)
            {
                inventory.addGadget(pickedUpObject);
            }
            if (weapon != null)
            {
                inventory.addWeapon(pickedUpObject);
            }
        }
    }
}
