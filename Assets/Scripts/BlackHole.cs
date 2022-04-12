using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mirror;
using Mirror.Experimental;


public class BlackHole : NetworkBehaviour
{
    public float range = 20f;
    public float duration = 5f;
    public Collider[] collisions;
    public float intensity = 1f;
    public float distance;
    Vector3 force;

    public List<GameObject> objects = new List<GameObject>();

    void FixedUpdate()
    {
        if (duration <= 0)
        {
            Destroy(this.gameObject);
            return;
        }
        collisions = Physics.OverlapSphere(this.transform.position, range);
        objects.Clear();

        foreach (var collision in collisions)
        {
            if (collision.gameObject.GetComponent<Rigidbody>() == true)
            {
                objects.Add(collision.gameObject);
            }
        }

        foreach (var item in objects)
        {
            if (NetworkClient.active)
            {
                var identity = item.GetComponent<NetworkIdentity>();
                if (!identity.isLocalPlayer)
                    continue;
            }
            else if (item.GetComponent<GameClient>() != null)
            {
                continue;
            }
            distance = Vector3.Distance(item.transform.position, transform.position);
            force = (transform.position - item.transform.position).normalized / distance * intensity;
            item.GetComponent<Rigidbody>().AddForce(force, ForceMode.Force);
        }
        duration = duration - Time.deltaTime;
    }
}
