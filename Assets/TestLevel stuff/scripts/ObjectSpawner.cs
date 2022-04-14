using Mirror;
using UnityEngine;

public class ObjectSpawner : NetworkBehaviour
{
    [SyncVar] [SerializeField] GameObject obejctToSpawn;
    [SerializeField] float spawnTime;
    [SerializeField] bool spawnAtStart;
    enum ObjectType { Weapon, Gadget, Object };
    [SerializeField]ObjectType objectType;


    GameObject spawnedObject;
    GameObject newSpawnedObject;
    bool spawned;
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

        else if (nextSpawnTime < NetworkTime.time)
        {
            spawnObject();
        }

    }
    [ServerCallback]
    public GameObject pickupObject(Inventory inventory)
    {
        print("pickup object");
        if (spawned)
        {
            //Weapon
            if (obejctToSpawn.GetComponent<IWeapon>() != null)
            {
                newSpawnedObject = inventory.currentWeapon;
                spawnedObject.GetComponent<NetworkIdentity>().AssignClientAuthority(inventory.connectionToClient);
            }
            //Gadget
            else if (obejctToSpawn.GetComponent<IGadget>() != null)
            {
                spawnedObject.GetComponent<NetworkIdentity>().AssignClientAuthority(inventory.connectionToClient);
                if (inventory.gadget)
                {
                    newSpawnedObject = inventory.gadget;
                }
                else
                {
                    nextSpawnTime = NetworkTime.time + spawnTime;
                    spawned = false;
                }
            }
            if (newSpawnedObject)
            {
                newSpawnedObject.GetComponent<NetworkIdentity>().RemoveClientAuthority();
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
 
        spawnedObject = Instantiate(obejctToSpawn, transform.position + new Vector3(0, 1, 0), Quaternion.identity);
        newSpawnedObject = spawnedObject;

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
