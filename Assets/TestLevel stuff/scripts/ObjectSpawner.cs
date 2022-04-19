using Mirror;
using UnityEngine;

public class ObjectSpawner : NetworkBehaviour
{
    [SyncVar] [SerializeField] GameObject objectToSpawn;
    [SerializeField] float spawnTime;
    [SerializeField] bool spawnAtStart;
    enum ObjectType { Weapon, Gadget, Object };
    [SerializeField]ObjectType objectType;


    GameObject spawnedObject;
    GameObject newSpawnedObject;
    bool spawned, objectIsReplaced;
    double nextSpawnTime = 0;

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

        if (spawnedObject != newSpawnedObject)
        {
            spawnedObject = newSpawnedObject;
            spawnedObject.GetComponent<NetworkIdentity>().RemoveClientAuthority();
        }

        if (spawned)
        {
            if (objectType == ObjectType.Gadget || objectType == ObjectType.Weapon)
            {
                spawnedObject.transform.Rotate(0.5f, 1, 0.5f);
            }
        }

        if (nextSpawnTime < NetworkTime.time)
        {
            if (!spawned || objectIsReplaced)
            {
                spawnObject();
            }
        }

    }
    [ServerCallback]
    public GameObject pickupObject(Inventory inventory)
    {
        //print("pickup object");
        if (spawned)
        {
            newSpawnedObject = null;
            nextSpawnTime = NetworkTime.time + spawnTime;
            //Weapon
            if (objectToSpawn.GetComponent<IWeapon>() != null)
            {
                newSpawnedObject = inventory.currentWeapon;
                spawnedObject.GetComponent<NetworkIdentity>().AssignClientAuthority(inventory.connectionToClient);
            }
            //Gadget
            else if (objectToSpawn.GetComponent<IGadget>() != null)
            {
                spawnedObject.GetComponent<NetworkIdentity>().AssignClientAuthority(inventory.connectionToClient);
                if (inventory.gadget)
                {
                    newSpawnedObject = inventory.gadget;
                }
                else
                {
                    spawned = false;
                }
            }
            if (newSpawnedObject)
            {
                objectIsReplaced = true;
                newSpawnedObject.GetComponent<NetworkIdentity>().RemoveClientAuthority();
                newSpawnedObject.transform.localScale = new Vector3(1, 1, 1);
                newSpawnedObject.transform.position = transform.position + new Vector3(0, 1, 0);
            }
            return spawnedObject;
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
        if (spawnedObject)
        {
            NetworkServer.Destroy(spawnedObject);
        }
        spawnedObject = Instantiate(objectToSpawn, transform.position + new Vector3(0, 1, 0), objectToSpawn.transform.rotation);
        newSpawnedObject = spawnedObject;
        objectIsReplaced = false;

        if (objectType == ObjectType.Gadget || objectType == ObjectType.Weapon)
        {
            foreach (Collider collider in spawnedObject.GetComponents<Collider>())
            {
                collider.enabled = false;
            }
        }

        NetworkServer.Spawn(spawnedObject);
        spawned = true;
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == spawnedObject)
        {
            objectWasRemoved();
        }
    }
}
