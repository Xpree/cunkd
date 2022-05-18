using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
public class InverseKinematicsController : MonoBehaviour
{
    [SerializeField] TwoBoneIKConstraint RightHandIK;
    [SerializeField] TwoBoneIKConstraint LeftHandIK;



    // Update is called once per frame
    void Update()
    {
        // how to change weight
        // ik.weight = 0;
    }
}
