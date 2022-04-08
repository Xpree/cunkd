using Mirror;
using UnityEngine;

public class ObjectSpawner : NetworkBehaviour
{
    [SerializeField] GameObject obejctToSpawn;
    [SerializeField] float spawnTime;
    [SerializeField] bool spawnAtStart;
    enum ObjectType { Weapon, Gadget, Object };
    [SerializeField]ObjectType objectType;

    NetworkTransform trans;

    GameObject spawnedObject;
    bool spawned;
    double nextSpawnTime = 0;

    [Client]
    private void Awake()
    {
        if (objectType == ObjectType.Object)
        {
            gameObject.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    [ServerCallback]
    private void FixedUpdate()
    {
        if (!spawnAtStart)
        {
            nextSpawnTime = NetworkTime.time + spawnTime;
            spawnAtStart = true;
        }

        if (spawned)
        {
            if (objectType == ObjectType.Gadget || objectType == ObjectType.Weapon)
            {
                spawnedObject.transform.Rotate(0.5f, 1, 0.5f);
            }
        }

        else if (nextSpawnTime < NetworkTime.time)
        {
            spawnObject();
        }
    }
    [ServerCallback]
    public GameObject pickupObject()
    {
        //print("picking up object");
        if (spawned)
        {
            nextSpawnTime = NetworkTime.time + spawnTime;

            if (objectType == ObjectType.Gadget || objectType == ObjectType.Weapon)
            {
                deSpawnObject();
                return obejctToSpawn;
            }
        }
        return null;
    }

    void objectWasRemoved()
    {
        nextSpawnTime = NetworkTime.time + spawnTime;
        spawned = false;
        spawnedObject = null;
    }


    [ServerCallback]
    public void spawnObject()
    {
        //print("spawning object...");
        spawnedObject = Instantiate(obejctToSpawn, transform.position + new Vector3(0, 1, 0), Quaternion.identity);

        if (objectType == ObjectType.Gadget || objectType == ObjectType.Weapon)
        {
            foreach (Collider collider in spawnedObject.GetComponents<Collider>())
            {
                collider.enabled = false;
            }
        }

        trans = spawnedObject.GetComponent<NetworkTransform>();
        NetworkServer.Spawn(spawnedObject);
        spawned = true;
    }
    [ServerCallback]
    public void deSpawnObject()
    {
        //print("despawning object...");
        NetworkServer.Destroy(spawnedObject);
        spawned = false;
        spawnedObject = null;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == spawnedObject)
        {
            objectWasRemoved();
        }
    }
}
