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
    [SerializeField] Collider interactionCollider;

    [Header("Diagnostics")]
    [SyncVar(hook = nameof(OnSpawnedItemChanged))] public GameObject spawnedItem;

    NetworkTimer nextSpawnTime;
    

    public bool IsEquipmentSpawner => objectToSpawn.GetComponent<NetworkItem>() != null;

    public Transform GetSpawnAnchor() => spawnAnchor != null ? spawnAnchor.transform : this.transform;

    private void OnEnable()
    {
        GetComponent<Collider>().enabled = !IsEquipmentSpawner;
        GetComponent<MeshRenderer>().enabled = IsEquipmentSpawner;
        interactionCollider.enabled = spawnedItem != null && spawnedItem.GetComponent<NetworkItem>() != null;

        if (IsEquipmentSpawner)
        {
            if(interactionCollider == null)
            {
                Debug.Log("Missing interact collider on " + this.name);
                return;
            }
            EventBus.Register(new EventHook(nameof(EventPlayerInteract), interactionCollider.gameObject), new System.Action<NetworkIdentity>(Pickup));
            EventBus.Register(new EventHook(nameof(EventPlayerInteractHoverStart), interactionCollider.gameObject), new System.Action<NetworkIdentity>(OnInteractHoverStart));
            EventBus.Register(new EventHook(nameof(EventPlayerInteractHoverStop), interactionCollider.gameObject), new System.Action<NetworkIdentity>(OnInteractHoverStop));
        }
            
    }

    private void OnDisable()
    {
        interactionCollider.enabled = false;
        if (IsEquipmentSpawner)
        {
            EventBus.Unregister(new EventHook(nameof(EventPlayerInteract), interactionCollider.gameObject), new System.Action<NetworkIdentity>(Pickup));
            EventBus.Unregister(new EventHook(nameof(EventPlayerInteractHoverStart), interactionCollider.gameObject), new System.Action<NetworkIdentity>(OnInteractHoverStart));
            EventBus.Unregister(new EventHook(nameof(EventPlayerInteractHoverStop), interactionCollider.gameObject), new System.Action<NetworkIdentity>(OnInteractHoverStop));
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
        interactionCollider.enabled = false;
        if (current != null)
        {
            var item = current.GetComponent<NetworkItem>();
            if (item != null)
                OnSpawnedItem(item);
        }
    }

    void OnInteractHoverStart(NetworkIdentity player)
    {
        FindObjectOfType<PlayerGUI>()?.interactiveButton(this);
    }

    void OnInteractHoverStop(NetworkIdentity player)
    {
        FindObjectOfType<PlayerGUI>()?.interactiveButton(null);
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
        //TODO: item.SetPositionWithRotationCenter(parent);
        item.transform.localRotation = Quaternion.identity;
        interactionCollider.enabled = true;
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
            interactionCollider.enabled = false;
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

    private void OnTriggerExit(Collider other)
    {
        if (spawnedItem && !IsEquipmentSpawner)
        {
            spawnedItem.GetComponent<Rigidbody>().useGravity = true;
        }
    }
}
