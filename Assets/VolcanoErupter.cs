using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class VolcanoErupter : NetworkBehaviour
{
    [SerializeField] float force;
    [SerializeField] Transform forcePosition;
    [SerializeField] GameObject spawnPositions;
    [SerializeField] GameObject[] spawnOnEruption;
    [SerializeField] float spawnInterval;
    [SerializeField] float numberOfRocks;

    Transform[] positions;
    double nextSpawn = 0;
    //private void Start()
    //{
    //    positions = spawnPositions.GetComponentsInChildren<Transform>();
    //    nextSpawn = NetworkTime.time + spawnInterval;
    //}

    //void Erupt()
    //{
    //    nextSpawn = NetworkTime.time + spawnInterval;
    //}

    //float spawnedRocks = 0;

    //private void Update()
    //{
    //    if (spawnedRocks++ <= numberOfRocks && NetworkTime.time <= nextSpawn)
    //    {
    //        Instantiate(spawnOnEruption[Random.Range(0, spawnOnEruption.Length)], positions[Random.Range(1, positions.Length)].position, Quaternion.identity);
    //        nextSpawn += spawnInterval;
    //    }
    //}

    //private void OnTriggerStay(Collider other)
    //{
    //    Rigidbody rigidbody = other.GetComponent<Rigidbody>();
    //    if (rigidbody)
    //    {
    //        rigidbody.AddForce((Vector3.up * force / Mathf.Abs(other.transform.position.y - forcePosition.position.y)), ForceMode.Impulse);
    //    }
    //}
}
