using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
public class animationStateController : NetworkBehaviour
{
    Animator animator;

    int isIdleHash;
    int isRunHash;

    public InputAction Move;
    PlayerInput input;
    PlayerControls Controls;
    public Vector2 _movementInput = Vector2.zero;
    public bool HasMovementInput => _movementInput.sqrMagnitude > 0;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }
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
