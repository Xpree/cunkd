using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class MakeHingeMovable : MonoBehaviour
{
    void MakeMovable()
    {
        //gameObject.AddComponent<Pullable>();
    }

    private void OnJointBreak(float breakForce)
    {
        print("hinge broke");
        MakeMovable();
    }
}
