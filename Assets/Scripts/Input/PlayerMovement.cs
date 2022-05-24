using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using Unity.VisualScripting;
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkTransform))]
public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] Animator animator;
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
    public float currentMaxSpeed => (_settings.CharacterMovement.MaxSpeed + (_client.IsCunkd ? _settings.CunkdSpeedBoost : 0)) * maxSpeedScaling;
    public float currentMaxFriction => _settings.CharacterMovement.FrictionAcceleration * maxFrictionScaling;

    public float gravityScaling => _settings.CharacterMovement.gravityScaling;

    public Vector2 _movementInput = Vector2.zero;

    public bool _landed;

    public GameObject _platform;
    public NetworkTransform _networkTransform;

    NetworkTimer _preventGroundFriction;

    GameClient _client;

    private void Awake()
    {
        _client = GetComponent<GameClient>();
        
        _networkTransform = GetComponent<NetworkTransform>();
        _rigidBody = GetComponent<Rigidbody>();
        _rigidBody.useGravity = false;
        _rigidBody.isKinematic = false;
    }
    
    private void Start()
    {
        if (_settings == null)
        {
            Debug.LogError("Missing GameSettings reference on " + name);
        }
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        _rigidBody.isKinematic = true;
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
    public bool HasGroundFriction => (_isGrounded || (HasCoyoteTime && HasMovementInput == false)) && _rigidBody.velocity.y < _settings.CharacterMovement.MaxSpeed * 0.5f && _preventGroundFriction.Elapsed > 0;

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
        _rigidBody.velocity += (gravityScaling * Time.fixedDeltaTime) * Physics.gravity;
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
        Util.SetJumpForce(_rigidBody, height, gravityScaling);
    }


    [Command]
    void CmdPerformedJump(bool airJump)
    {
        var trigger = airJump ? nameof(EventPlayerAirJumped) : nameof(EventPlayerJumped);
        NetworkEventBus.TriggerExcludeOwner(trigger, this.netIdentity);
        if(isLocalPlayer)
        {
            animator.SetBool("jump", true);
        }
    }
    void PerformJump()
    {
        if(_rigidBody.velocity.y < 0.5)
        {
            if(isLocalPlayer)
            {
                animator.SetBool("jump", false);
            }
        }
        if (!_performJump)
            return;
        _performJump = false;
        SetKinematicOff();

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
            if(isLocalPlayer)
            {
                animator.SetBool("jump", false);
            }
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

    private void FixedUpdate()
    {
        // NOTE: Runs on all clients
        
        //ApplyPlatformRelativeMovement();
      
        CheckGrounded();

        if(isLocalPlayer)
        {
            if (_platform != null)
            {
                if (_isGrounded)
                {
                    this.transform.parent = _platform.transform;
                }
                _platform = null;
            }
            else
            {
                this.transform.parent = null;
            }
        }


        ApplyGravity();
        PerformJump();
        if (HasGroundFriction)
        {
            ApplyFriction();
        }

        if (HasMovementInput)
        {
            SetKinematicOff();
            ApplyAcceleration(_movementInput);
            if(isLocalPlayer)
            {
                animator.SetBool("run", true);
            }
        }
        if(isLocalPlayer)
        {
            if(!HasMovementInput)
            {
                animator.SetBool("run", false);
            }
        }
        //dance
        if(isLocalPlayer)
        {
            if(UnityEngine.InputSystem.Keyboard.current[Key.P].isPressed)
            {   
                animator.SetBool("dance", true);
            }
            else{
                animator.SetBool("dance", false);
            }
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
        Debug.Log("Force = " + force.magnitude);
        if (force.magnitude >= 35f)
        {
            FMODUnity.RuntimeManager.PlayOneShotAttached("event:/SoundStudents/SFX/Environment/Cat sound when dying", this.gameObject);
        }
        _rigidBody.AddForce(force, mode);
        _isGrounded = false;
        _preventGroundFriction = NetworkTimer.FromNow(0.5f);
    }

    public void NoFriction()
    {
        _isGrounded = false;
        _preventGroundFriction = NetworkTimer.FromNow(0.5f);
    }

    public void SetKinematicOff()
    {
        if(_rigidBody.isKinematic)
        {
            _rigidBody.isKinematic = false;
            _rigidBody.velocity = Vector3.zero;
        }
    }

    System.Collections.IEnumerator PreventMovement()
    {
        _rigidBody.isKinematic = true;
        yield return new WaitForSeconds(2.0f);
        SetKinematicOff();
    }

    [TargetRpc]
    public void TargetRespawn(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
        ResetState();
        _networkTransform.CmdTeleport(position, rotation);
        _client.CmdRespawnComplete();

        StopAllCoroutines();
        StartCoroutine(PreventMovement());
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
    
    private void OnCollisionEnter(Collision collision)
    {
        Renderer rend = collision.gameObject.GetComponent<Renderer>();
        if (rend)
        {
            if (rend.material.name == "wood (Instance)")
            {
                Debug.Log("walking on wood");
                FMODUnity.RuntimeManager.PlayOneShotAttached("event:/SoundStudents/SFX/Environment/Step sounds on brigde", this.gameObject);
            }
            else if (rend.material.name == "BrownGrey (Instance)")
            {
                Debug.Log("walking on sand");
                FMODUnity.RuntimeManager.PlayOneShotAttached("event:/SoundStudents/SFX/Environment/Step sounds on dirt", this.gameObject);
            }
            else
            {
                Debug.Log("walking on concrete sound");
                FMODUnity.RuntimeManager.PlayOneShotAttached("event:/SoundStudents/SFX/Environment/Step sounds on concrete", this.gameObject);
            }
        }
    }
}
