using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using Unity.VisualScripting;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkTransform))]
public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] GameSettings _settings;
    Rigidbody _rigidBody;

    // NOTE: underscored variables is not intended to be used outside this class.
    // They are only public to be able to see them in Unity editor.
    // If you need to know about them consider triggering events when they change.

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

    public bool _landed;

    public Animator _localAnimator;

    public GameObject _platform;
    public GameObject _lastPlatform;
    public Vector3 _lastPlatformPosition = Vector3.zero;
    public GameObject body;
    // TODO: Make rotation relative movement
    //public Quaternion _lastPlatformRotation = Quaternion.identity;

    public NetworkTransform _networkTransform;

    private void Awake()
    {
        _networkTransform = GetComponent<NetworkTransform>();
        _localAnimator = GetComponent<Animator>();
        _rigidBody = GetComponent<Rigidbody>();
        _rigidBody.useGravity = false;
        _rigidBody.isKinematic = false;
    }
    
    private void Start()
    {
        if(isLocalPlayer)
        {
            body.SetActive(false);
        }
        if (_settings == null)
        {
            Debug.LogError("Missing GameSettings reference on " + name);
        }
    }

    void ResetState()
    {
        _networkTransform.Reset();
        _rigidBody.velocity = Vector3.zero;
        _isGrounded = false;
        _groundNormal = Vector3.up;
        _airJumped = false;
        _performJump = false;
        _landed = false;
        _lastGrounded = 0;
        _lastJump = 0;
        _movementInput = Vector2.zero;
        maxSpeedScaling = 1f;
        maxFrictionScaling = 1f;
    }

    public bool HasStrongAirControl => NetworkTime.time - _lastJump <= _settings.CharacterMovement.StrongAirControlTime;
    public bool HasCoyoteTime => (NetworkTime.time - _lastGrounded <= _settings.CharacterMovement.CoyoteTime && _lastGrounded - _lastJump >= _settings.CharacterMovement.CoyoteTime);

    public bool HasGroundContact => (_isGrounded || HasCoyoteTime);

    public bool HasMovementInput => _movementInput.sqrMagnitude > 0;
    public bool HasGroundFriction => (_isGrounded || (HasCoyoteTime && HasMovementInput == false)) && _rigidBody.velocity.y < _settings.CharacterMovement.MaxSpeed * 0.5f;

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


    [Command]
    void CmdPerformedJump(bool airJump)
    {
        var trigger = airJump ? nameof(EventPlayerAirJumped) : nameof(EventPlayerJumped);
        NetworkEventBus.TriggerExcludeOwner(trigger, this.netIdentity);
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

        var trigger = _airJumped ? nameof(EventPlayerAirJumped) : nameof(EventPlayerJumped);
        EventBus.Trigger(trigger, this.gameObject);
        CmdPerformedJump(_airJumped);
        _lastJump = NetworkTime.time;
        ApplyJumpForce(_settings.CharacterMovement.JumpHeight);
    }

    void SetLanded(bool value)
    {
        bool trigger = !_landed && value;
        _landed = value;
        if(trigger)
        {
            EventBus.Trigger(nameof(EventPlayerLanded), this.gameObject);
            _localAnimator.SetTrigger("land");
        }
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
        SetLanded(_isGrounded);
    }

    
    void ApplyPlatformRelativeMovement()
    {
        if (_platform == null || !_isGrounded)
        {
            _lastPlatform = null;
            return;
        }

        if (_lastPlatform != _platform)
        {
            _lastPlatform = _platform;
            _lastPlatformPosition = _platform.transform.position;

            
            //_lastPlatformRotation = _platform.transform.localRotation;
            return;
        }

        Vector3 pos = _platform.transform.position;
        Vector3 delta = pos - _lastPlatformPosition;
        _lastPlatformPosition = pos;
        _rigidBody.MovePosition(_rigidBody.position + delta);

        
        /*
        Quaternion rot = _platform.transform.localRotation;
        Quaternion deltaRot = rot * Quaternion.Inverse(_lastPlatformRotation);
        Vector3 angles = new Vector3(0, deltaRot.eulerAngles.y, 0);
        transform.Rotate(angles);
        _lastPlatformRotation = rot;
        */
       
        _platform = null;
    }

    
    private void FixedUpdate()
    {
        // NOTE: Runs on all clients
        
        ApplyPlatformRelativeMovement();
        
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
        _networkTransform.CmdTeleport(position, rotation);
    }


    void OnTeleport(Vector3 position)
    {
        transform.position = position;
        ResetState();
    }

    [ClientRpc]
    void RpcTeleport(Vector3 position)
    {
        if(this.isClientOnly)
            OnTeleport(position);
    }

    [Server]
    public void Teleport(Vector3 position)
    {
        OnTeleport(position);
        RpcTeleport(position);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Platform"))
        {
            _platform = other.gameObject;
        }
    }
}
