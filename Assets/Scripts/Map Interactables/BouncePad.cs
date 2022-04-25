using Mirror;
using UnityEngine;

public class BouncePad : NetworkBehaviour
{
    [SerializeField] float cooldown;
    [SerializeField] float jumpHeight;
    [SerializeField] GameObject sphere;
    double nextLaunch =0;
    bool isReady = true;


    private void FixedUpdate()
    {
        if (!isReady && nextLaunch < NetworkTime.time)
        {
            ActivatePad();        
        }
    }

    void LaunchObject(Rigidbody rb)
    {
        if(Util.HasPhysicsAuthority(rb.gameObject))
        {
            Util.SetJumpForce(rb, jumpHeight);
        }
        sphere.SetActive(true);
        nextLaunch = NetworkTime.time + cooldown;
        isReady = false;
    }

    void ActivatePad()
    {
        sphere.SetActive(false);
        isReady = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();

        if (isReady)
        {
            {
                LaunchObject(rb);
            }
        }
    }
}
