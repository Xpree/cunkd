using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footstep : MonoBehaviour
{
    public PlayerMovement movement;

    public void PlayFootstepSound() 
    {
        movement.PlayStepSound();
    }
}
