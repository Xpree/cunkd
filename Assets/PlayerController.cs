using Fusion;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public static PlayerController Local { get; set; }

    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;

    private NetworkCharacterControllerPrototype _cc;
    [Networked] private Vector3 _moveDirection { get; set; }

    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterControllerPrototype>();
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
            Local = this;
    }


    // Updates the networked variables to move the player
    void UpdateInput()
    {
        if (GetInput(out NetworkInputData input))
        {
            if(input.move != Vector2.zero)
            {
                _moveDirection = Quaternion.Euler(0.0f, input.rotation, 0.0f) * Vector3.forward;
            }
            else
            {
                _moveDirection = Vector3.zero;
            }
        }
    }


    // Uses the networked variables to move the player
    void Move()
    {
        _cc.Move(_moveDirection);
    }

    public override void FixedUpdateNetwork()
    {
        UpdateInput();
        Move();
    }

}