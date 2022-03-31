using Fusion;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public static PlayerController Local { get; set; }

    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;

    private NetworkCharacterControllerPrototype _cc;
    private Vector3 _moveDirection;

    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterControllerPrototype>();
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
            Local = this;
    }

    public void SetDirections(Vector3 moveDirection)
    {
        _moveDirection = moveDirection;
    }

    public void Move()
    {
        
        _cc.Move(_moveDirection);
        
    }
}