using Mirror;
using UnityEngine;
using Unity.VisualScripting;

public class ObjectSpawner : NetworkBehaviour
{
    [SerializeField] GameObject objectToSpawn;
    [SerializeField] GameObject spawnAnchor;
    [SerializeField] float spawnTime;
    [SerializeField] bool spawnAtStart;
    enum ObjectType { Weapon, Gadget, Object };
    [SerializeField] ObjectType objectType;
    [SerializeField] Collider interactColldier;

    [Header("Diagnostics")]
    [SyncVar(hook = nameof(OnSpawnedItemChanged))] public GameObject spawnedItem;

    NetworkTimer nextSpawnTime;
    

    public bool IsEquipmentSpawner => objectToSpawn.GetComponent<NetworkItem>() != null;

    public Transform GetSpawnAnchor() => spawnAnchor != null ? spawnAnchor.transform : this.transform;

    private void OnEnable()
    {
        GetComponent<MeshRenderer>().enabled = IsEquipmentSpawner;
        interactColldier.enabled = spawnedItem != null && spawnedItem.GetComponent<NetworkItem>() != null;

        if (IsEquipmentSpawner)
        {
            EventBus.Register(new EventHook(nameof(EventPlayerInteract), this.gameObject), new System.Action<NetworkIdentity>(Pickup));
        }
            
    }

    private void OnDisable()
    {
        interactColldier.enabled = false;
        if (IsEquipmentSpawner)
            EventBus.Unregister(new EventHook(nameof(EventPlayerInteract), this.gameObject), new System.Action<NetworkIdentity>(Pickup));
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
        interactColldier.enabled = false;
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
        item.SetPositionWithRotationCenter(parent);
        item.transform.localRotation = Quaternion.identity;
        interactColldier.enabled = true;
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
            spawnedItem = null;
            nextSpawnTime = NetworkTimer.FromNow(spawnTime);
            interactColldier.enabled = false;
        }
    }


    public void Pickup(NetworkIdentity actor)
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
            else
            {
                Debug.Log("TODO: Unable to pickup feedback.");
            }
        }
        else
        {
            Debug.Log("Nothing to pick up.");
        }
    }

}
