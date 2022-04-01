using Fusion;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public static PlayerController Local { get; set; }

    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;

    private Rigidbody _rigidBody;
    [Networked] private float _rotation { get; set; }
    [Networked] private bool _jump { get; set; }
    [Networked] private bool _move { get; set; }

    [SerializeField] float rotationSpeed = 15.0f;
    [SerializeField] float movementSpeed = 6.0f;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
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
            _rotation = input.rotation;
            _jump = input.jump;
            _move = input.move != Vector2.zero;
        }
    }


    // Uses the networked variables to move the player
    void Move()
    {
        float deltaTime = Runner.DeltaTime;
        var travelRotation = Quaternion.Euler(0.0f, _rotation, 0.0f);
        _rigidBody.rotation = Quaternion.Slerp(_rigidBody.rotation, travelRotation, rotationSpeed * deltaTime);        

        Vector3 velocity = _rigidBody.velocity;
        if(this._move)
        {
            Vector3 direction = travelRotation * Vector3.forward;
            Vector2 movement = new Vector2(direction.x, direction.z);
            Vector2 horizontalVelocity = new Vector2(velocity.x, velocity.z);
            horizontalVelocity += movement;

            horizontalVelocity = Vector2.ClampMagnitude(horizontalVelocity, movementSpeed);

            velocity.x = horizontalVelocity.x;
            velocity.z = horizontalVelocity.y;
        }

        if(this._jump)
        {
            velocity.y += 6.0f;
        }

        _rigidBody.velocity = velocity;
    }

    public override void FixedUpdateNetwork()
    {
        UpdateInput();
        Move();
    }

}