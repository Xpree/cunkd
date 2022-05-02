using Mirror;
using UnityEngine;

public class ObjectSpawner : NetworkBehaviour, IInteractable
{
    [SerializeField] GameObject objectToSpawn;
    [SerializeField] GameObject spawnAnchor;
    [SerializeField] float spawnTime;
    [SerializeField] bool spawnAtStart;
    enum ObjectType { Weapon, Gadget, Object };
    [SerializeField] ObjectType objectType;


    [Header("Diagnostics")]
    [SyncVar(hook = nameof(OnSpawnedItemChanged))] public NetworkItem spawnedItem;

    NetworkTimer nextSpawnTime;

    public Transform GetSpawnAnchor() => spawnAnchor != null ? spawnAnchor.transform : this.transform;

    public bool IsGadgetSpawner => objectToSpawn != null && objectToSpawn.GetComponent<IGadget>() != null;
    public bool IsWeaponSpawner => objectToSpawn != null && objectToSpawn.GetComponent<IWeapon>() != null;
    public bool IsPowerUpSpawner => !IsGadgetSpawner && !IsWeaponSpawner;

    private void Awake()
    {
        if (IsPowerUpSpawner)
        {
            gameObject.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    public override void OnStartServer()
    {
        if (spawnAtStart)
        {
            SpawnObject();
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if(spawnedItem != null)
        {
            OnSpawnedItem(spawnedItem);
        }

    }
    void OnSpawnedItemChanged(NetworkItem previous, NetworkItem current)
    {
        if (current != null)
            OnSpawnedItem(current);
    }

    private void FixedUpdate()
    {
        if (spawnedItem)
        {
            if (objectType == ObjectType.Gadget || objectType == ObjectType.Weapon)
            {
                spawnAnchor.transform.Rotate(0.5f, 1, 0.5f);
            }
        }
        else if (NetworkServer.active && nextSpawnTime.HasTicked)
        {
            SpawnObject();
        }
    }

    [Server]
    public void SpawnObject()
    {
        if (spawnedItem)
        {
            NetworkServer.Destroy(spawnedItem.gameObject);
            spawnedItem = null;
        }

        var anchor = GetSpawnAnchor();
        var go = Instantiate(objectToSpawn, anchor.position, anchor.rotation);
        NetworkServer.Spawn(go);

        spawnedItem = go.GetComponent<NetworkItem>();
        if (spawnedItem != null)
            OnSpawnedItem(spawnedItem);
    }

    void OnSpawnedItem(NetworkItem item)
    {
        var parent = GetSpawnAnchor();
        item.transform.parent = parent;
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
    }


    [Command(requiresAuthority = false)]
    void CmdPickup(NetworkIdentity actor)
    {
        if (spawnedItem != null && (actor?.GetComponent<INetworkItemOwner>()?.CanPickup(spawnedItem) ?? false))
        {
            spawnedItem.Pickup(actor);
            spawnedItem = null;
            nextSpawnTime = NetworkTimer.FromNow(spawnTime);
        }
    }

    void IInteractable.Interact(NetworkIdentity actor)
    {
        if (actor != null && spawnedItem != null)
        {
            if (actor.GetComponent<INetworkItemOwner>()?.CanPickup(spawnedItem) ?? false)
            {
                CmdPickup(actor);
            }
        }
        else
        {
            Debug.Log("Nothing to interact.");
        }
    }

    //private void OnTriggerExit(Collider other)
    //{
    //    if (spawnedItem && IsPowerUpSpawner)
    //    {
    //        spawnedItem.GetComponent<Rigidbody>().useGravity = true;
    //    }
    //}
}
