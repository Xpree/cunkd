using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

[RequireComponent(typeof(Rigidbody))]
public class FPSPlayerController2 : NetworkBehaviour
{
    private Rigidbody playerBody;
    public bool isGrounded = false;
    public Vector3 GroundNormal = Vector3.up;

    private bool airJumped = false;

    bool performJump = false;

    private bool wPressed = false;
    private bool aPressed = false;
    private bool sPressed = false;
    private bool dPressed = false;

    public float maxSpeed = 9.0f;
    public float decelerationSpeed = 27f;
    public float jumpHeight = 1.8f;


    public LayerMask GroundedMask;

    private void Awake()
    {
        playerBody = GetComponent<Rigidbody>();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        playerBody.isKinematic = false;
    }

    void ApplyGravity()
    {
        playerBody.velocity += Physics.gravity * Time.fixedDeltaTime;
    }

    void ApplyFriction()
    {
        var vel = playerBody.velocity;
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
        playerBody.velocity = vel;
    }

    void ApplyAcceleration()
    {
        float moveX = (dPressed ? 1 : 0) - (aPressed ? 1 : 0);
        float moveZ = (wPressed ? 1 : 0) - (sPressed ? 1 : 0);

        Vector3 velocity = playerBody.velocity;
        velocity.y = 0;
        float terminalSpeed = Mathf.Max(velocity.magnitude, maxSpeed);
        Vector3 velocityChange = (moveX * transform.right + moveZ * transform.forward).normalized * maxSpeed;
        if (!isGrounded)
        {
            velocityChange *= Time.fixedDeltaTime;
        }
        velocity += velocityChange;

        velocity = Vector3.ClampMagnitude(velocity, terminalSpeed);
        velocity.y = playerBody.velocity.y;
        playerBody.velocity = velocity;
    }

    void PerformJump()
    {
        if (!performJump)
            return;
        performJump = false;

        if (!isGrounded)
        {
            if (airJumped)
                return;
            airJumped = true;
        } 
        else
        {
            airJumped = false;
        }

        float jumpForce = Mathf.Sqrt(Mathf.Abs((2.0f * playerBody.mass * Physics.gravity.y) * jumpHeight));
        var vel = playerBody.velocity;
        vel.y = jumpForce;
        playerBody.velocity = vel;
    }


    private void OnCollisionStay(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; ++i)
        {
            var contact = collision.GetContact(i);
            if (Vector3.Dot(contact.normal, Vector3.up) > 0.8)
            {
                isGrounded = true;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer) { return; }

        ApplyGravity();
        PerformJump();

        if ((wPressed ^ sPressed) == false && (dPressed ^ aPressed) == false)
        {
            if(isGrounded && playerBody.velocity.y < 0)
            {
                ApplyFriction();
            }
        }
        else
        {
            ApplyAcceleration();
        }
    }


    public void Jump(InputAction.CallbackContext contextState)
    {
        if (!isLocalPlayer) { return; }
        if (contextState.performed)
        {
            performJump = true;
        }

    }

    public void PlayerMoveForward(InputAction.CallbackContext contextState)
    {
        if (contextState.performed)
        {
            wPressed = true;
        }
        else if(contextState.canceled)
        {
            wPressed = false;
        }
    }

    public void PlayerMoveRight(InputAction.CallbackContext contextState)
    {
        if (contextState.performed)
        {
            dPressed = true;
        }
        else if (contextState.canceled)
        {
            dPressed = false;
        }

    }

    public void PlayerMoveLeft(InputAction.CallbackContext contextState)
    {
        if (contextState.performed)
        {
            aPressed = true;
        }
        else if (contextState.canceled)
        {
            aPressed = false;
        }

    }

    public void PlayerMoveBackward(InputAction.CallbackContext contextState)
    {
        if (contextState.performed)
        {
            sPressed = true;
        }
        else if (contextState.canceled)
        {
            sPressed = false;
        }

    }

    [TargetRpc]
    public void TargetAddforce(Vector3 force, ForceMode mode)
    {
        playerBody.AddForce(force, mode);
        isGrounded = false;
    }

    [TargetRpc]
    public void TRpcSetPosition(Vector3 position)
    {
        transform.position = position;
    }


    private void OnGUI()
    {
        if (!isLocalPlayer) { return; }

        GUI.Box(new Rect(Screen.width * 0.5f - 1, Screen.height * 0.5f - 1, 2, 2), GUIContent.none);
    }

    private void Update()
    {
        if (Keyboard.current[Key.E].wasPressedThisFrame)
        {
            shootRay();
        }
    }

    void shootRay()
    {
        RaycastHit hit;
        //print("shooting ray");
        Camera cam = gameObject.GetComponentInChildren<PlayerCameraController>().playerCamera;
        Ray ray = cam.ScreenPointToRay(new Vector2(Screen.width,Screen.height)/2);
        if (Physics.Raycast(ray.origin, ray.direction, out hit, 15))
        {
            print("object hit: " + hit.transform.gameObject);

            ObjectSpawner objectSpawner = hit.transform.gameObject.GetComponent<ObjectSpawner>();
            if (objectSpawner)
            {
                CmdPickupObject(objectSpawner);
            }
        }
    }

    [Command]
    void CmdPickupObject(ObjectSpawner objectSpawner)
    {
        GameObject pickedUpObject = objectSpawner.pickupObject();
        Inventory inventory = gameObject.GetComponent<Inventory>();

        ScoreCard scorecard = gameObject.GetComponent<ScoreCard>();
        IGadget gadget = pickedUpObject.GetComponent<IGadget>();

        if (pickedUpObject.name == "Extra Life")
        {
            scorecard.livesLeft++;
        }
        if (gadget != null)
        {
            inventory.addGadget(pickedUpObject);
        }
    }
}
