using Mirror;
using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.VFX;

public class ObjectSpawner : NetworkBehaviour
{
    [SerializeField] GameObject objectToSpawn;
    [SerializeField] GameObject spawnAnchor;
    [SerializeField] float spawnTime;
    [SerializeField] bool spawnAtStart;
    enum ObjectType { Weapon, Gadget, Object };
    [SerializeField] ObjectType objectType;
    [SerializeField] NetworkAnimator effect;

    [Header("Diagnostics")]
    [SyncVar(hook = nameof(OnSpawnedItemChanged))] public GameObject spawnedItem;

    NetworkTimer nextSpawnTime;

    public bool IsEquipmentSpawner => objectToSpawn.GetComponent<NetworkItem>() != null;

    public Transform GetSpawnAnchor() => spawnAnchor != null ? spawnAnchor.transform : this.transform;

    private void OnEnable()
    {
        if (!IsEquipmentSpawner)
            gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        GetComponent<Collider>().enabled = !IsEquipmentSpawner;
        GetComponent<MeshRenderer>().enabled = IsEquipmentSpawner;
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

    void CheckSpawnedItem()
    {
        if (spawnedItem == null)
        {
            return;
        }
        var item = spawnedItem.GetComponent<NetworkItem>();
        if (item != null && item.IsSpawned == false)
        {
            // Reset Item
            effect.SetTrigger("stopReady");
            spawnedItem = null;
            nextSpawnTime = NetworkTimer.FromNow(spawnTime);
        }
    }
    
    [ServerCallback]
    private void FixedUpdate()
    {
        CheckSpawnedItem();
        
        if (spawnedItem == null && nextSpawnTime.HasTicked)
        {
            SpawnObject();
        }
    }

    [Server]
    public void SpawnObject()
    {
        var anchor = GetSpawnAnchor();
        var go = Instantiate(objectToSpawn, anchor.position, anchor.rotation);
        NetworkServer.Spawn(go);
        nextSpawnTime = NetworkTimer.FromNow(spawnTime);

        spawnedItem = go;
        var item = spawnedItem.GetComponent<NetworkItem>();
        if (item != null)
        {
            effect.SetTrigger("startReady");
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

    void OnSpawnedItem(NetworkItem item)
    {        
        var anchor = GetSpawnAnchor();
        item.OnSpawned(anchor);
    }

    private void OnTriggerExit(Collider other)
    {
        if (spawnedItem && !IsEquipmentSpawner)
        {
            spawnedItem.GetComponent<Rigidbody>().useGravity = true;
        }
    }
}
