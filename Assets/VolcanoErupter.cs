using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class VolcanoErupter : NetworkBehaviour
{
    [SerializeField] float force;
    [SerializeField] float spawnInterval;
    [SerializeField] Transform forcePosition;
    [SerializeField] GameObject spawnPositions;
    [SerializeField] GameObject[] objectsToSpawn;
    [SerializeField] Collider forceCollider;

    Transform[] positions;
    double nextSpawn = 0;



    [Server]
    private void Start()
    {
        positions = spawnPositions.GetComponentsInChildren<Transform>();
        forceCollider.enabled = false;
        //nextSpawn = NetworkTime.time + spawnInterval;
        nextSpawn = GameStats.RoundTimer.Elapsed + spawnInterval;
    }

    [Server]
    public void Erupt(double duration, GameObject[] objectsToSpawn, float spawnInterval, int maxSpawns)
    {
        //print("Volcano erupting");
        forceCollider.enabled = true;
        this.duration = duration;
        this.objectsToSpawn = objectsToSpawn;
        this.spawnInterval = spawnInterval;
        this.maxSpawns = maxSpawns;
        this.duration = GameStats.RoundTimer.Elapsed + duration;
        nextSpawn = GameStats.RoundTimer.Elapsed + spawnInterval;
        spawnedRocks = 0;
    }

    int maxSpawns = 0;
    double duration = 0;
    float spawnedRocks = 0;

    [Server]
    private void Update()
    {
        if (GameStats.RoundTimer.Elapsed <= duration)
        {
            if (spawnedRocks++ <= maxSpawns && nextSpawn <= GameStats.RoundTimer.Elapsed)
            {
                NetworkServer.Spawn(Instantiate(objectsToSpawn[Random.Range(0, objectsToSpawn.Length)], positions[Random.Range(1, positions.Length)].position, Quaternion.identity));
                nextSpawn += spawnInterval;
            }
        }
        else
        {
            forceCollider.enabled = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        Rigidbody rigidbody = other.GetComponent<Rigidbody>();
        if (rigidbody)
        {
            rigidbody.AddForce((Vector3.up * force / Mathf.Abs(other.transform.position.y - forcePosition.position.y)), ForceMode.Impulse);
        }
    }
}
