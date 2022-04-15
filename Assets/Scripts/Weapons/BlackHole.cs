using UnityEngine;
using Mirror;

public class BlackHole : NetworkBehaviour
{
    public float range = 20f;
    public float duration = 5f;
    public Collider[] collisions;
    public float intensity = 1f;
    public float distance;
    Vector3 force;

    void FixedUpdate()
    {
        if (duration <= 0)
        {
            if(NetworkServer.active)
            {
                NetworkServer.Destroy(this.gameObject);
            }
            return;
        }
        collisions = Physics.OverlapSphere(this.transform.position, range);

        foreach (var collision in collisions)
        {
            var rb = collision.gameObject.GetComponent<Rigidbody>();
            if (rb != null && Util.HasPhysicsAuthority(collision.gameObject))
            {
                distance = Vector3.Distance(rb.transform.position, transform.position);
                force = (transform.position - rb.transform.position).normalized / distance * intensity;
                rb.AddForce(force, ForceMode.Force);
            }
        }

        duration = duration - Time.fixedDeltaTime;
    }
}
