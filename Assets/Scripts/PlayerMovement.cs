using UnityEngine;
using Mirror;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : NetworkBehaviour
{

    // TODO Move to scripted object
    public float maxSpeed = 9.0f;
    public float decelerationSpeed = 27f;
    public float jumpHeight = 1.8f;


    GameInputs _inputs;

    Rigidbody _rigidBody;
    bool _airJumped = false;
    bool _performJump = false;
    

    [Header("Diagnostics")]
    public bool IsGrounded = false;
    public Vector3 GroundNormal = Vector3.up;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        _rigidBody.isKinematic = false;
        _inputs = FindObjectOfType<GameInputs>();
    }

    void ApplyGravity()
    {
        _rigidBody.velocity += Physics.gravity * Time.fixedDeltaTime;
    }

    void ApplyFriction()
    {
        var vel = _rigidBody.velocity;
        vel.y = 0;
        var speed = Mathf.Max(vel.magnitude - decelerationSpeed * Time.fixedDeltaTime);
        if (speed <= 0)
        {
            vel = Vector3.zero;
        }
        else
        {
            vel = vel.normalized * speed;
        }

        vel.y = Physics.gravity.y * Time.fixedDeltaTime;
        _rigidBody.velocity = vel;
    }

    void ApplyAcceleration()
    {
        Vector2 move = _inputs.Move;

        Vector3 velocity = _rigidBody.velocity;
        velocity.y = 0;
        float terminalSpeed = Mathf.Max(velocity.magnitude, maxSpeed);
        Vector3 velocityChange = (move.x * transform.right + move.y * transform.forward).normalized * maxSpeed;
        if (!IsGrounded)
        {
            velocityChange *= Time.fixedDeltaTime;
        }
        velocity += velocityChange;

        velocity = Vector3.ClampMagnitude(velocity, terminalSpeed);
        velocity.y = _rigidBody.velocity.y;
        _rigidBody.velocity = velocity;
    }

    void PerformJump()
    {
        if (!_performJump)
            return;
        _performJump = false;

        if (!IsGrounded)
        {
            if (_airJumped)
            {
                return;
            }
            _airJumped = true;
        } 
        else
        {
            _airJumped = false;
        }

        float jumpForce = Mathf.Sqrt(Mathf.Abs((2.0f * _rigidBody.mass * Physics.gravity.y) * jumpHeight));
        var vel = _rigidBody.velocity;
        vel.y = jumpForce;
        _rigidBody.velocity = vel;
    }


    private void OnCollisionStay(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; ++i)
        {
            var contact = collision.GetContact(i);
            if (Vector3.Dot(contact.normal, Vector3.up) > 0.8)
            {
                IsGrounded = true;
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
            if(IsGrounded && _rigidBody.velocity.y < 0)
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
