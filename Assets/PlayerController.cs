using Fusion;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public static PlayerController Local { get; set; }

    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;

    private Rigidbody _rigidBody;
    private Animator _animator;

    [Networked] private float _rotation { get; set; }
    [Networked] private bool _jump { get; set; }
    [Networked] private bool _move { get; set; }

    [Header("Player")]
    [Tooltip("Move speed of the character in m/s")]
    public float _movementSpeed = 6.0f;
    [Tooltip("Rotation speed of the character in m/s")]
    public float _rotationSpeed = 15.0f;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not.")]
    public bool Grounded = true;
    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;
    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;
    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;


    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

    private float _animationBlend = 0;
    private bool _animateJump = false;
    private float _animateFreeFallTimeout = 0;

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        AssignAnimationIDs();
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
            _jump = input.jump;
            _move = input.move != Vector2.zero;
            if(_move)
            {
                _rotation = input.rotation;
            }
        }
    }


    // Uses the networked variables to move the player
    void Move()
    {
        float deltaTime = Runner.DeltaTime;
        var travelRotation = Quaternion.Euler(0.0f, _rotation, 0.0f);
        _rigidBody.rotation = Quaternion.Slerp(_rigidBody.rotation, travelRotation, _rotationSpeed * deltaTime);        

        Vector3 velocity = _rigidBody.velocity;
        if(this._move)
        {
            Vector3 direction = travelRotation * Vector3.forward;
            Vector2 movement = (new Vector2(direction.x, direction.z)).normalized;
            Vector2 horizontalVelocity = new Vector2(velocity.x, velocity.z);
            horizontalVelocity += movement * 10.0f;
            horizontalVelocity = Vector2.ClampMagnitude(horizontalVelocity, _movementSpeed);

            velocity.x = horizontalVelocity.x;
            velocity.z = horizontalVelocity.y;
        }

        if(this._jump && Grounded)
        {
            velocity.y += 6.0f;
            _animateJump = true;
        }

        _rigidBody.velocity = velocity;
    }

    void GroundCheck()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
    }


    void UpdateAnimator()
    {

        _animator.SetBool(_animIDGrounded, Grounded);

        if(Grounded)
        {
            _animateFreeFallTimeout = 0.15f;
        } else
        {
            _animateFreeFallTimeout -= Runner.DeltaTime;
        }

        float verticalVelocity = _rigidBody.velocity.y;

        bool falling = verticalVelocity < 0 && !Grounded;
        bool jumping = verticalVelocity > 0 && !Grounded;

        if(_animateJump)
        {
            if(falling)
            {
                _animateJump = false;
            }
        }

        _animator.SetBool(_animIDFreeFall, falling && verticalVelocity < 1 && _animateFreeFallTimeout < 0);
        _animator.SetBool(_animIDJump, jumping && _animateJump);
        _animator.SetFloat(_animIDMotionSpeed, 1.0f); // Analog movement?

        float horizontalVelocity = new Vector2(_rigidBody.velocity.x, _rigidBody.velocity.z).magnitude;
        _animationBlend = Mathf.Lerp(_animationBlend, horizontalVelocity, Runner.DeltaTime * _movementSpeed);
        _animator.SetFloat(_animIDSpeed, horizontalVelocity);
    }

    public override void FixedUpdateNetwork()
    {
        UpdateInput();
        GroundCheck();
        Move();
        UpdateAnimator();
    }
}