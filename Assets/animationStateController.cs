using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
public class animationStateController : NetworkBehaviour
{
    [SerializeField] Animator animator;

    void Update()
    {
        if(isLocalPlayer)
        {
            if(UnityEngine.InputSystem.Keyboard.current[Key.W].isPressed)
            {
                animator.SetBool("run", true);
            }
            else if(UnityEngine.InputSystem.Keyboard.current[Key.S].isPressed)
            {
                animator.SetBool("run", true);
            }
            else if(UnityEngine.InputSystem.Keyboard.current[Key.D].isPressed)
            {
                animator.SetBool("run", true);
            }
            else if(UnityEngine.InputSystem.Keyboard.current[Key.A].isPressed)
            {
                animator.SetBool("run", true);
            }
            else
            {
                animator.SetBool("run", false);
            }
            if(UnityEngine.InputSystem.Keyboard.current[Key.P].isPressed)
            {
                animator.SetBool("dance", true);
            }
            else{
                animator.SetBool("dance", false);
            }
            if(UnityEngine.InputSystem.Keyboard.current[Key.Space].isPressed)
            {
                animator.SetBool("jump", true);
            }
            else{
                animator.SetBool("jump", false);
            }
        }

    }
}
