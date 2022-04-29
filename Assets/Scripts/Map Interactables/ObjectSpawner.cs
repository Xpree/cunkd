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
    [SyncVar(hook = nameof(OnSpawnedItemChanged))] public GameObject spawnedItem;

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

        if (spawnedItem != null)
        {
            var item = spawnedItem.GetComponent<NetworkItem>();
            if (item != null)
                OnSpawnedItem(item);
        }

    }
    void OnSpawnedItemChanged(GameObject previous, GameObject current)
    {

        if (current != null)
        {
            var item = current.GetComponent<NetworkItem>();
            if (item != null)
                OnSpawnedItem(item);
        }
    }

    [ServerCallback]
    private void FixedUpdate()
    {
        if (spawnedItem == null && nextSpawnTime.HasTicked)
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
        nextSpawnTime = NetworkTimer.FromNow(spawnTime);

        spawnedItem = go;
        var item = spawnedItem.GetComponent<NetworkItem>();
        if (item != null)
            OnSpawnedItem(item);
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
        if (spawnedItem == null)
            return;
        var item = spawnedItem.GetComponent<NetworkItem>();
        if (item != null && (actor?.GetComponent<INetworkItemOwner>()?.CanPickup(item) ?? false))
        {
            item.Pickup(actor);
            item = null;
            nextSpawnTime = NetworkTimer.FromNow(spawnTime);
        }
    }

    void IInteractable.Interact(NetworkIdentity actor)
    {
        if (actor != null && spawnedItem != null)
        {
            var item = spawnedItem.GetComponent<NetworkItem>();
            if (item == null)
                return;
            if (actor.GetComponent<INetworkItemOwner>()?.CanPickup(item) ?? false)
            {
                CmdPickup(actor);
            }
        }
        else
        {
            Debug.Log("Nothing to interact.");
        }
    }
}
