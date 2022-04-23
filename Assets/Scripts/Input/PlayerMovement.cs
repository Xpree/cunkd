using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] GameSettings _settings;

    Rigidbody _rigidBody;

    [Header("Diagnostics")]
    public bool _isGrounded = false;
    public Vector3 _groundNormal = Vector3.up;
    public bool _airJumped = false;
    public bool _performJump = false;
    public double _lastGrounded = 0;
    public double _lastJump = 0;

    public float maxSpeedScaling = 1f;
    public float maxFrictionScaling = 1f;
    public float currentMaxSpeed => _settings.CharacterMovement.MaxSpeed * maxSpeedScaling;
    public float currentMaxFriction => _settings.CharacterMovement.DecelerationSpeed * maxFrictionScaling;

    public Vector2 _movementInput = Vector2.zero;

    private void Start()
    {
        if (_settings == null)
        {
            Debug.LogError("Missing GameSettings reference on " + name);
        }
    }

    void ResetState()
    {
        _rigidBody.velocity = Vector3.zero;
        _isGrounded = false;
        _groundNormal = Vector3.up;
        _airJumped = false;
        _performJump = false;
        _lastGrounded = 0;
        _lastJump = 0;
        _movementInput = Vector2.zero;
        maxSpeedScaling = 1f;
        maxFrictionScaling = 1f;
    }

    public bool HasStrongAirControl => NetworkTime.time - _lastJump <= _settings.CharacterMovement.StrongAirControlTime;
    public bool HasCoyoteTime => (NetworkTime.time - _lastGrounded <= _settings.CharacterMovement.CoyoteTime && _lastGrounded - _lastJump >= _settings.CharacterMovement.CoyoteTime);

    public bool HasGroundContact => _isGrounded || HasCoyoteTime;

    public bool HasMovementInput => _movementInput.sqrMagnitude > 0;
    public bool HasGroundFriction => (_isGrounded || (HasCoyoteTime && HasMovementInput == false)) && _rigidBody.velocity.y < 0;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _rigidBody.useGravity = false;
        _rigidBody.isKinematic = false;
    }

    public Vector3 HorizontalVelocity
    {
        get
        {
            var vel = _rigidBody.velocity;
            vel.y = 0;
            return vel;
        }

        set
        {
            _rigidBody.velocity = new Vector3(value.x, _rigidBody.velocity.y, value.z);
        }
    }

    void ApplyGravity()
    {
        _rigidBody.velocity += (_rigidBody.mass * Time.fixedDeltaTime) * Physics.gravity;
    }

    void ApplyFriction()
    {
        var vel = this.HorizontalVelocity;
        var speed = Mathf.Max(vel.magnitude - currentMaxFriction * Time.fixedDeltaTime);
        if (speed <= 0)
        {
            vel = Vector3.zero;
        }
        else
        {
            vel = vel.normalized * speed;
        }

        this.HorizontalVelocity = vel;
    }

    void ApplyAcceleration(Vector2 move)
    {
        Vector3 velocityChange = (move.x * transform.right + move.y * transform.forward).normalized * currentMaxSpeed;
        if (!_isGrounded && !HasStrongAirControl)
        {
            // Air acceleration
            velocityChange *= _settings.CharacterMovement.AirMovementMultiplier * Time.fixedDeltaTime;
        }

        Vector3 velocity = this.HorizontalVelocity;
        float terminalSpeed = Mathf.Max(velocity.magnitude, currentMaxSpeed);
        velocity += velocityChange;
        // Makes sure the player can't increase its speed beyond its previous speed or maxSpeed which ever is greater.
        velocity = Vector3.ClampMagnitude(velocity, terminalSpeed);
        this.HorizontalVelocity = velocity;
    }

    void ApplyJumpForce(float height)
    {
        float jumpForce = Mathf.Sqrt(Mathf.Abs((2.0f * _rigidBody.mass * Physics.gravity.y) * height));
        var vel = _rigidBody.velocity;
        vel.y = jumpForce;
        _rigidBody.velocity = vel;
    }

    void PerformJump()
    {
        if (!_performJump)
            return;
        _performJump = false;

        if (!HasGroundContact)
        {
            if (_airJumped)
            {
                return;
            }
            _airJumped = true;
        }

        _lastJump = NetworkTime.time;
        ApplyJumpForce(_settings.CharacterMovement.JumpHeight);
    }


    private void OnCollisionStay(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; ++i)
        {
            var contact = collision.GetContact(i);
            if (Vector3.Dot(contact.normal, Vector3.up) > 0.2)
            {
                _isGrounded = true;
                _airJumped = false;
                _lastGrounded = NetworkTime.time;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        _isGrounded = false;
    }

    private void FixedUpdate()
    {
        ApplyGravity();
        PerformJump();

        if (HasGroundFriction)
        {
            ApplyFriction();
        }

        if (HasMovementInput)
        {
            ApplyAcceleration(_movementInput);
        }
    }

    public void OnMoveAction(InputAction.CallbackContext ctx)
    {
        _movementInput = ctx.ReadValue<Vector2>();
    }

    public void OnJumpAction(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            _performJump = true;
    }


    [TargetRpc]
    public void TargetAddforce(Vector3 force, ForceMode mode)
    {
        _rigidBody.AddForce(force, mode);
        _isGrounded = false;
    }

    [TargetRpc]
    public void TargetRespawn(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
        ResetState();
    }


    [TargetRpc]
    public void TargetTeleport(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
        ResetState();
    }

    [TargetRpc]
    public void TargetTeleport(Vector3 position)
    {
        transform.position = position;
        ResetState();
    }


    [Command(requiresAuthority = false)]
    public void CmdTeleport(Vector3 position)
    {
        transform.position = position;
        TargetTeleport(position);
    }


    public void Teleport(Vector3 position)
    {
        transform.position = position;
        if (NetworkServer.active)
        {
            TargetTeleport(position);
        }
        else
        {
            CmdTeleport(position);
        }
    }
}
