using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CoconutBreak : NetworkBehaviour
{
    [SerializeField] float breakVelocity;
    [SerializeField] int breakChance;
    [SerializeField] GameObject coconutTopHalf;
    [SerializeField] GameObject coconutBottomHalf;
    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void BreakCheck()
    {
        //print("breakcheck. current velocity: " + rb.velocity.magnitude);
        int breakVal = Random.Range(0, 100);
        if (breakVelocity < rb.velocity.magnitude)
        {
            //print("trying to break coconut. current velocity: " + rb.velocity.magnitude + " breakval: "+ breakVal);
            if (breakVal < breakChance)
            {
                breakCoconut();
            }
        }
    }

    void breakCoconut()
    {

        var go1 = Instantiate(coconutTopHalf, transform.position, Quaternion.identity);
        go1.GetComponent<Rigidbody>().velocity = (rb.velocity.magnitude - breakVelocity + 10) * Vector3.right;
        NetworkServer.Spawn(go1);

        var go2 = Instantiate(coconutBottomHalf, transform.position, Quaternion.identity);
        go2.GetComponent<Rigidbody>().velocity = (rb.velocity.magnitude - breakVelocity + 10) * Vector3.left;
        NetworkServer.Spawn(go2);

        NetworkServer.Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        BreakCheck();
    }
}
