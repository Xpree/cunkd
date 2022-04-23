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


    GameObject spawnedObject;
    NetworkTimer nextSpawnTime;

    public Transform GetSpawnAnchor() => spawnAnchor != null ? spawnAnchor.transform : this.transform;

    public bool IsGadgetSpawner => objectToSpawn?.GetComponent<IGadget>() != null;
    public bool IsWeaponSpawner => objectToSpawn?.GetComponent<IWeapon>() != null;
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

    private void FixedUpdate()
    {
        if (spawnedObject)
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
        if (spawnedObject)
        {
            NetworkServer.Destroy(spawnedObject);
            spawnedObject = null;
        }

        var go = Instantiate(objectToSpawn, GetSpawnAnchor());
        NetworkServer.Spawn(go);
        OnSpawned(go);
        RpcSetSpawned(go);
    }

    void OnSpawned(GameObject go)
    {
        spawnedObject = go;
        if (go)
        {
            go.transform.parent = GetSpawnAnchor();
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
        }
    }

    [ClientRpc]
    void RpcSetSpawned(GameObject go)
    {
        if (isClientOnly)
        {
            OnSpawned(go);
        }
    }


    void ResetSpawn()
    {
        spawnedObject = null;
        nextSpawnTime = NetworkTimer.FromNow(spawnTime);
    }


    [Command(requiresAuthority = false)]
    void CmdPickup(NetworkIdentity actor)
    {
        var networkItem = spawnedObject?.GetComponent<NetworkItem>();
        if (networkItem != null && (actor?.GetComponent<INetworkItemOwner>()?.CanPickup(networkItem) ?? false))
        {
            networkItem.Pickup(actor);
            ResetSpawn();
        }
    }

    void IInteractable.Interact(NetworkIdentity actor)
    {
        var networkItem = spawnedObject?.GetComponent<NetworkItem>();
        if (actor != null && networkItem != null)
        {
            if (actor?.GetComponent<INetworkItemOwner>()?.CanPickup(networkItem) ?? false)
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
