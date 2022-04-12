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

    [ServerCallback]
    void FixedUpdate()
    {
        foreach (var item in objects)
        {
            var controller = item.GetComponent<FPSPlayerController>();
            if (controller != null)
            {
                controller.PhysicsAuthority(true);
            }
        }

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
            var controller = item.GetComponent<FPSPlayerController>();
            if (controller != null)
            {
                controller.PhysicsAuthority(false);
            }
            distance = Vector3.Distance(item.transform.position, transform.position);
            force = (transform.position - item.transform.position).normalized / distance * intensity;
            item.GetComponent<Rigidbody>().AddForce(force, ForceMode.Force);
        }
        duration = duration - Time.deltaTime;
    }
}
