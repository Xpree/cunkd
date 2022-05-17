using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.VFX;

public class BeamSphere : NetworkBehaviour
{
    [SerializeField] NetworkAnimator animator;
    private void Awake()
    {
        animator.SetTrigger("Beam");
    }
}
