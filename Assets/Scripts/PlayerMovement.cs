using UnityEngine;
using Mirror;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : NetworkBehaviour
{

    // TODO Move to scripted object
    public float maxSpeed = 9.0f;
    public float decelerationSpeed = 27f;
    public float jumpHeight = 1.8f;
    public float airMovementMultiplier = 1.0f;

    public double coyoteTime = 1.0f;
    public double strongAirControlTime = 0.1f;

    GameInputs _inputs;

    Rigidbody _rigidBody;


    [Header("Diagnostics")]
    public bool IsGrounded = false;
    public Vector3 GroundNormal = Vector3.up;
    public bool _airJumped = false;
    public bool _performJump = false;
    public double _lastGrounded = 0;
    public double _lastJump = 0;

    public bool HasStrongAirControl => NetworkTime.time - _lastJump <= strongAirControlTime;
    public bool HasCoyoteTime => (NetworkTime.time - _lastGrounded <= coyoteTime && _lastGrounded - _lastJump >= coyoteTime);


    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _rigidBody.useGravity = false;
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        _rigidBody.isKinematic = false;        
        _inputs = FindObjectOfType<GameInputs>();
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
        var speed = Mathf.Max(vel.magnitude - decelerationSpeed * Time.fixedDeltaTime);
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

    void ApplyAcceleration()
    {
        Vector2 move = _inputs.Move;

        Vector3 velocityChange = (move.x * transform.right + move.y * transform.forward).normalized * maxSpeed;
        if (!IsGrounded && !HasStrongAirControl)
        {
            // Air acceleration
            velocityChange *= airMovementMultiplier * Time.fixedDeltaTime;
        }

        Vector3 velocity = this.HorizontalVelocity;
        float terminalSpeed = Mathf.Max(velocity.magnitude, maxSpeed);
        velocity += velocityChange;
        // Makes sure the player can't increase it's speed beyond it's previous speed or maxSpeed which ever is greater.
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

        if(!IsGrounded && !HasCoyoteTime)
        {
            if (_airJumped)
            {
                return;
            }
            _airJumped = true;
        }

        _lastJump = NetworkTime.time;
        ApplyJumpForce(jumpHeight);
    }


    private void OnCollisionStay(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; ++i)
        {
            var contact = collision.GetContact(i);
            if (Vector3.Dot(contact.normal, Vector3.up) > 0.8)
            {
                IsGrounded = true;
                _airJumped = false;
                _lastGrounded = NetworkTime.time;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        IsGrounded = false;
    }

    [ClientCallback]
    private void FixedUpdate()
    {
        if (!isLocalPlayer) { return; }

        ApplyGravity();
        PerformJump();

        if (_inputs.MovePressed)
        {
            ApplyAcceleration();
        }
        else
        {
            if (IsGrounded && _rigidBody.velocity.y < 0)
            {
                ApplyFriction();
            }
        }
    }

    [ClientCallback]
    private void Update()
    {
        if (!isLocalPlayer) { return; }

        if (_inputs.Jump)
        {
            _performJump = true;
        }
    }


    [TargetRpc]
    public void TargetAddforce(Vector3 force, ForceMode mode)
    {
        _rigidBody.AddForce(force, mode);
        IsGrounded = false;
    }

    [TargetRpc]
    public void TargetRespawn(Vector3 position)
    {
        transform.position = position;
        _rigidBody.velocity = Vector3.zero;
        _performJump = false;
        _airJumped = false;
        IsGrounded = false;
        GroundNormal = Vector3.up;
    }

}
