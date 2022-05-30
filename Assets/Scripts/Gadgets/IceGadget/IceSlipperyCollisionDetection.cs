using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceSlipperyCollisionDetection : MonoBehaviour
{
    [SerializeField] IceGadgetTrap igt;
    [SerializeField] SpreadMat sm;

    private void OnTriggerStay(Collider other)
    {
        if (igt && other.tag == "Player")
        {
            igt.makePlayerSlip(other.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (sm)
        {
            sm.setCollision(other);
        }
    }
}
