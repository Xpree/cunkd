using Mirror;
using UnityEngine;


public class ItemSpawner : NetworkBehaviour
{
    [SerializeField] GameObject obejctToSpawn;
    [SerializeField] float spawnTime;
    [SerializeField] bool spawnAtStart;


    bool spawned;
    GameObject spawnedObject;
    double nextSpawnTime = 0;

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
            spawnedObject.transform.Rotate(0.5f, 1, 0.5f);
        }
        else if (nextSpawnTime < NetworkTime.time)
        {
            spawnObject();
        }
    }

    [ServerCallback]
    public void pickupObject()
    {
        //print("picking up object");
        if (spawned)
        {
            nextSpawnTime = NetworkTime.time + spawnTime;
            deSpawnObject();
        }
    }
    [ServerCallback]
    public void spawnObject()
    {
        //print("spawning object...");
        spawnedObject = Instantiate(obejctToSpawn, transform.position + new Vector3(0, 1, 0), Quaternion.identity);
        foreach (Collider collider in spawnedObject.GetComponents<Collider>())
        {
            collider.enabled = false;
        } 
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
}
