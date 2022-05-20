using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Unity.VisualScripting;

public class VolcanoErupter : NetworkBehaviour
{
    [SerializeField] float force;
    [SerializeField] float spawnInterval;
    [SerializeField] Transform forcePosition;
    [SerializeField] GameObject spawnPositions;
    [SerializeField] GameObject[] objectsToSpawn;
    [SerializeField] Collider forceCollider;
    [SerializeField] CameraShakeSource cameraShake;
    
    Transform[] positions;
    double nextSpawn = 0;

    [Server]
    private void Start()
    {
        positions = spawnPositions.GetComponentsInChildren<Transform>();
        forceCollider.enabled = false;
    }

    [ClientRpc]
    void RpcCameraShake(NetworkTimer eventTime)
    {
        cameraShake.OneShotShake(eventTime);
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
        this.duration = GameStats.RoundTimer + duration;
        nextSpawn = GameStats.RoundTimer + spawnInterval;
        spawnedRocks = 0;
        RpcCameraShake(NetworkTimer.Now);
    }

    int maxSpawns = 0;
    double duration = -1;
    float spawnedRocks = 0;

    [Server]
    private void Update()
    {
        if (GameStats.RoundTimer <= duration && spawnedRocks <= maxSpawns)
        {
            if (nextSpawn <= GameStats.RoundTimer)
            {
                NetworkServer.Spawn(Instantiate(objectsToSpawn[Random.Range(0, objectsToSpawn.Length)], positions[Random.Range(1, positions.Length)].position, Quaternion.identity));
                spawnedRocks++;
                nextSpawn += spawnInterval;
            }
        }
        else
        {
            forceCollider.enabled = false;
        }
    }

    [ClientRpc]
    void addForceToPlayer(GameObject go)
    {
        go.GetComponent<Rigidbody>().AddForce((Vector3.up * force / Mathf.Abs(go.transform.position.y - forcePosition.position.y)), ForceMode.Impulse);
    }

    [Server]
    private void OnTriggerStay(Collider other)
    {

        Rigidbody rigidbody = other.GetComponent<Rigidbody>();
        if (rigidbody)
        {
            if (rigidbody.gameObject.GetComponent<PlayerMovement>())
            {
                addForceToPlayer(other.gameObject);
            }
            else
            {
                rigidbody.AddForce((Vector3.up * force / Mathf.Abs(other.transform.position.y - forcePosition.position.y)), ForceMode.Impulse);
            }
        }
    }
}




[UnitTitle("On Volcano Erupt")]
[UnitCategory("Events\\Level")]
public class EventVolcanoErupt : GameObjectEventUnit<EmptyEventArgs>
{
    public override System.Type MessageListenerType => null;

    protected override string hookName => nameof(EventCameraShake);
}