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
    public float currentMaxFriction => _settings.CharacterMovement.FrictionAcceleration * maxFrictionScaling;

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
    public bool HasGroundFriction => (_isGrounded || (HasCoyoteTime && HasMovementInput == false)) && _rigidBody.velocity.y < _settings.CharacterMovement.MaxSpeed * 0.5f;

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

            var max = _settings.CharacterMovement.TerminalVelocity;
            _rigidBody.velocity = Vector3.ClampMagnitude(new Vector3(value.x, _rigidBody.velocity.y, value.z), max);
        }
    }

    void ApplyGravity()
    {
        _rigidBody.velocity += (_rigidBody.mass * Time.fixedDeltaTime) * Physics.gravity;
    }


    void ApplyFriction()
    {
        var vel = this.HorizontalVelocity;
        var speed = vel.magnitude;
        var frictionAccel = _settings.CharacterMovement.FrictionAcceleration * maxFrictionScaling * Time.fixedDeltaTime;
        var friction = Mathf.Max(speed, _settings.CharacterMovement.FrictionMinSpeed) * frictionAccel;
        var newSpeed = speed - friction;
        if (newSpeed <= 0 || float.IsNormal(newSpeed) == false)
        {
            vel = Vector3.zero;
        }
        else
        {
            vel = vel.normalized * newSpeed;
        }

        this.HorizontalVelocity = vel;
    }

    // Quake style acceleration
    static Vector3 QuakeAccelerate(Vector3 velocity, Vector3 wishDir, float wishSpeed, float accel)
    {
        var currentSpeed = Vector3.Dot(velocity, wishDir);
        var addSpeed = Mathf.Clamp(wishSpeed - currentSpeed, 0, accel * wishSpeed * Time.fixedDeltaTime);
        return velocity + addSpeed * wishDir;
    }

    void Accelerate(Vector3 wishDir, float wishSpeed, float accel)
    {
        var addVelocity = accel * wishSpeed * Time.fixedDeltaTime * wishDir;

        Vector3 velocity = this.HorizontalVelocity;
        float terminalSpeed = Mathf.Max(velocity.magnitude, currentMaxSpeed);
        velocity += addVelocity;
        // Makes sure the player can't increase its speed beyond its previous speed or maxSpeed which ever is greater.
        velocity = Vector3.ClampMagnitude(velocity, terminalSpeed);

        this.HorizontalVelocity = velocity;
    }

    void ApplyAcceleration(Vector2 move)
    {
        Vector3 wishDir = (move.x * transform.right + move.y * transform.forward).normalized;
        float wishSpeed = _settings.CharacterMovement.MaxSpeed;

        float acceleration = maxSpeedScaling;

        if (_isGrounded || HasStrongAirControl)
        {
            acceleration *= _settings.CharacterMovement.GroundAcceleration;
        }
        else
        {
            acceleration *= _settings.CharacterMovement.AirAcceleration;
        }

        //_rigidBody.velocity = QuakeAccelerate(_rigidBody.velocity, wishDir, wishSpeed, acceleration);
        Accelerate(wishDir, wishSpeed, acceleration);
    }



    public void ApplyJumpForce(float height)
    {
        Util.SetJumpForce(_rigidBody, height);
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

    void CheckGrounded()
    {
        var m = _settings.CharacterMovement;
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - m.GroundedOffset, transform.position.z);
        _isGrounded = Physics.CheckSphere(spherePosition, m.GroundedRadius, m.GroundLayers, QueryTriggerInteraction.Ignore);
        if(_isGrounded)
        {
            _airJumped = false;
            _lastGrounded = NetworkTime.time;
        }
    }

    private void FixedUpdate()
    {
        CheckGrounded();
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

        // Temp reset
        maxSpeedScaling = 1f;
        maxFrictionScaling = 1f;
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
    public void TargetTeleport(Vector3 position)
    {
        transform.position = position;
        ResetState();
        CmdTeleportComplete();
    }

    [Command]
    void CmdTeleportComplete()
    {
        Util.SetClientPhysicsAuthority(GetComponent<NetworkIdentity>(), true);
    }


    [Server]
    public void Teleport(Vector3 position)
    {
        Util.SetClientPhysicsAuthority(GetComponent<NetworkIdentity>(), false);
        transform.position = position;
        ResetState();
        if(this.connectionToClient != null)
            TargetTeleport(position);
    }
}
