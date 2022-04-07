using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;


[RequireComponent(typeof(NetworkTransform))]
[RequireComponent(typeof(Rigidbody))]
public class FPSPlayerController : NetworkBehaviour
{


    private Rigidbody playerBody;
    private bool isGrounded = false;
    private bool onFloor = false;
    private bool wallRiding = false;
    private bool airJumped = false;
    Vector3 wallNormal = new Vector3();

    private bool wPressed = false;
    private bool aPressed = false;
    private bool sPressed = false;
    private bool dPressed = false;

    // playerspeed / max speed needs rethinking
    private float playerSpeed = 30.0f;
    private float maxSpeed = 15.0f;
    private float decelerationSpeed = 0.1f;
    private float slowMovementPenalty = 0.2f;
    private float jumpHeight = 2.5f;

    private void Awake()
    {
        playerBody = GetComponent<Rigidbody>();
    }


    private void FixedUpdate()
    {
        Vector3 totalInputVector = new (0, 0, 0);
        Vector3 playerMovementVector = new Vector3(0, 0, 0);
        Vector3 currentMoveVector = playerBody.velocity;

        if (!isLocalPlayer) { return; }

        if (wPressed)
        {
            if (isGrounded)
            {
                Vector3 moveVector = transform.forward;
                totalInputVector += (moveVector);
            }
            else
            {
                Vector3 moveVector = transform.forward;
                totalInputVector += (moveVector * slowMovementPenalty);
            }
        }

        if (dPressed)
        {
            if (isGrounded)
            {
                Vector3 moveVector = transform.right;
                totalInputVector += (moveVector * 0.8f);
            }
            else
            {
                Vector3 moveVector = transform.right;
                totalInputVector += (moveVector * 0.8f * slowMovementPenalty);
            }

        }

        if (aPressed)
        {
            if(isGrounded)
            {
                Vector3 moveVector = transform.right;
                totalInputVector -= (moveVector * 0.8f);
            }
            else
            {
                Vector3 moveVector = transform.right;
                totalInputVector -= (moveVector * 0.8f * slowMovementPenalty);
            }

        }

        if (sPressed)
        {
            if (isGrounded)
            {
                Vector3 moveVector = transform.forward;
                totalInputVector -= (moveVector * 0.5f);
            }
            else
            {
                Vector3 moveVector = transform.forward;
                totalInputVector -= (moveVector * 0.5f * slowMovementPenalty);
            }

        }

        Vector3 velocity = playerBody.velocity;
        float yVel = velocity.y;
        velocity.y = 0;
        float terminalSpeed = Mathf.Max(velocity.magnitude, maxSpeed);
        Vector3 velocityChange = totalInputVector.normalized * maxSpeed;
        if(!isGrounded)
        {
            velocityChange *= Time.fixedDeltaTime;
        }
        velocity += velocityChange;
      
        // Deceleration calculation if no player input
        // If applied to walls, allows players to slow/halt their fall when next to walls   
        if (isGrounded && playerBody.velocity.magnitude > 1 &&
            !wPressed && !dPressed && !sPressed && !aPressed)
        {
            Vector3 decelerateVector = new Vector3(playerBody.velocity.x, 0, playerBody.velocity.z);
            //Debug.Log("Decelerating");
            velocity -= (decelerateVector * decelerationSpeed);
        }

        velocity = Vector3.ClampMagnitude(velocity, terminalSpeed);
        velocity.y = yVel;
        playerBody.velocity = velocity;


        // THE BELOW COMMENTED-OUT CODE IS TEST-CODE FOR IMPROVING HIGH-SPEED PLAYER MOVEMENT CONTROLS

        //float impulseX = totalInputVector.x; bool impulseXPositive = impulseX >= 0 ? true : false;
        //float impulseZ = totalInputVector.z; bool impulseZPositive = impulseZ >= 0 ? true : false;
        //float currentXVelocity = currentMoveVector.x;
        //float currentZVelocity = currentMoveVector.z;

        //float percentXImpulse = 0.0f;
        //float percentZImpulse = 0.0f;
        //if (impulseZ < 0.01 && impulseZ > 0.01)
        //{
        //    percentXImpulse = 1;
        //    percentZImpulse = 0;
        //}
        //else if (impulseX < 0.01 && impulseX > 0.01)
        //{
        //    percentXImpulse = 0;
        //    percentZImpulse = 1;
        //}
        //else
        //{
        //    percentXImpulse = impulseX / (impulseX + impulseZ);
        //    percentZImpulse = impulseZ / (impulseZ + impulseX);
        //}
        //float XRatioOfMaxSpeed = maxSpeed * percentXImpulse;
        //float ZRatioOfMaxSpeed = maxSpeed * percentZImpulse;


        //if (impulseXPositive && currentXVelocity < maxSpeed)
        //{
        //    if (Mathf.Abs(impulseX) > Mathf.Abs(impulseZ))
        //    {
        //        playerMovementVector += new Vector3(impulseX, 0, 0);
        //    }
        //    else
        //    {
        //        playerMovementVector += new Vector3(impulseX, 0, 0);
        //    }

        //}
        //else if (!impulseXPositive && currentXVelocity > -maxSpeed)
        //{
        //    if (Mathf.Abs(impulseX) > Mathf.Abs(impulseZ))
        //    {
        //        playerMovementVector += new Vector3(impulseX, 0, 0);
        //    }
        //    else
        //    {
        //        playerMovementVector += new Vector3(impulseX, 0, 0);
        //    }
        //}

        //if (impulseZPositive && currentZVelocity < maxSpeed)
        //{
        //    if (Mathf.Abs(impulseZ) > Mathf.Abs(impulseX))
        //    {
        //        playerMovementVector += new Vector3(0, 0, impulseZ);
        //    }
        //    else
        //    {
        //        playerMovementVector += new Vector3(0, 0, impulseZ);
        //    }

        //}
        //else if (!impulseZPositive && currentZVelocity > -maxSpeed)
        //{
        //    if (Mathf.Abs(impulseZ) > Mathf.Abs(impulseX))
        //    {
        //        playerMovementVector += new Vector3(0, 0, impulseZ);
        //    }
        //    else
        //    {
        //        playerMovementVector += new Vector3(0, 0, impulseZ);
        //    }
        //}

        //if (playerMovementVector.x > 0)
        //{
        //    Mathf.Clamp(playerMovementVector.x, 0, maxSpeed * XRatioOfMaxSpeed);
        //}
        //else
        //{
        //    Mathf.Clamp(playerMovementVector.x, -maxSpeed * XRatioOfMaxSpeed, 0);
        //}

        //if(playerMovementVector.z > 0)
        //{
        //    Mathf.Clamp(playerMovementVector.z, 0, maxSpeed * ZRatioOfMaxSpeed);
        //}
        //else
        //{
        //    Mathf.Clamp(playerMovementVector.z, -maxSpeed * ZRatioOfMaxSpeed, 0);
        //}

        //Debug.Log("playermOveVector: " + playerMovementVector.x.ToString() + ", " + playerMovementVector.y.ToString() + ", " + playerMovementVector.z.ToString());

        //playerBody.AddForce(totalInputVector.normalized * playerSpeed * Time.fixedDeltaTime, ForceMode.Impulse);

    }


    public void Jump(InputAction.CallbackContext contextState)
    {
        if (contextState.performed)
        {
            if (isGrounded)
            {
                //Debug.Log("AIR JUMP");
                float jumpForce = Mathf.Sqrt(Mathf.Abs((2.0f * playerBody.mass * Physics.gravity.y) * jumpHeight));
                playerBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
            else if (wallRiding)
            {
                airJumped = false;
                playerBody.velocity = new Vector3(playerBody.velocity.x, 0.0f, playerBody.velocity.z);
                float jumpForce = Mathf.Sqrt(Mathf.Abs((2.0f * playerBody.mass * Physics.gravity.y) * jumpHeight));
                playerBody.AddForce((Vector3.up + wallNormal) * jumpForce, ForceMode.Impulse);
                wallNormal = new Vector3(0, 0, 0);
            }
            else if (airJumped == false)
            {
                //Debug.Log("AIR JUMP");
                playerBody.velocity = new Vector3(playerBody.velocity.x, 0.0f, playerBody.velocity.z);
                float jumpForce = Mathf.Sqrt(Mathf.Abs((2.0f * playerBody.mass * Physics.gravity.y) * jumpHeight));
                playerBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                airJumped = true;
            }
           
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


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "PlayArea")
        {
            isGrounded = true;
            onFloor = true;
            wallRiding = false;
            airJumped = false;
            //Debug.Log("Grounded!");
        }
        else if (collision.gameObject.transform.parent && collision.gameObject.transform.parent.gameObject.name == "map")
        {
            List<ContactPoint> contactList = new List<ContactPoint>();
            collision.GetContacts(contactList);

            // Testing one collisionPoint normal to getface orientation of surface collided with
            Vector3 collisionNormal = contactList[0].normal;
            float upTestNormal = Vector3.Dot(transform.up, collisionNormal);
            float rightTestNormal = Vector3.Dot(transform.right, collisionNormal);
            float leftTestNormal = Vector3.Dot(-transform.right, collisionNormal);

            if (upTestNormal > rightTestNormal && upTestNormal > leftTestNormal)
            {
                isGrounded = true;
                airJumped = false;
                //Debug.Log("Grounded!");
            }
            else
            {
                //Debug.Log("Hit a Wall!");
                wallRiding = true;
                wallNormal = collisionNormal;
            }

        }
    }


    [TargetRpc]
    public void TRpcSetPosition(Vector3 position)
    {
        transform.position = position;
    }


    private void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.name == "PlayArea")
        {
            isGrounded = false;
            onFloor = false;
            //Debug.Log("Airborne");
        }
        else if (collision.gameObject.transform.parent && collision.gameObject.transform.parent.gameObject.name == "map")
        {
            if (!onFloor)
            {
                isGrounded = false;
                //Debug.Log("Airborne");
            }
            wallRiding = false;
            wallNormal = new Vector3 (0, 0, 0);

        }
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

        ScoreCard scorecard = gameObject.GetComponent<ScoreCard>();

        if (pickedUpObject.name == "Extra Life")
        {
            scorecard.UpdateLives(scorecard.getLives() + 1);
        }
    }


}
