using UnityEngine;
using UnityEngine.InputSystem;

namespace Mirror.Examples.NetworkRoom
{
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(NetworkTransform))]
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : NetworkBehaviour
    {
        public CharacterController characterController;

        void OnValidate()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();

            characterController.enabled = false;
            GetComponent<Rigidbody>().isKinematic = true;
            GetComponent<NetworkTransform>().clientAuthority = true;
        }

        public override void OnStartLocalPlayer()
        {
            characterController.enabled = true;
        }

        [Header("Movement Settings")]
        public float moveSpeed = 8f;
        public float turnSensitivity = 5f;
        public float maxTurnSpeed = 100f;

        [Header("Diagnostics")]
        public float horizontal;
        public float vertical;
        public float turn;
        public float jumpSpeed;
        public bool isGrounded = true;
        public bool isFalling;
        public Vector3 velocity;

        void Update()
        {
            if (!isLocalPlayer || characterController == null || !characterController.enabled)
                return;


            vertical = Keyboard.current[Key.W].isPressed ? 1.0f : 0;
            vertical -= Keyboard.current[Key.S].isPressed ? 1.0f : 0;
            horizontal = Keyboard.current[Key.D].isPressed ? 1.0f : 0;
            horizontal -= Keyboard.current[Key.A].isPressed ? 1.0f : 0;
            // Q and E cancel each other out, reducing the turn to zero
            if (Keyboard.current[Key.Q].isPressed)
                turn = Mathf.MoveTowards(turn, -maxTurnSpeed, turnSensitivity);
            if (Keyboard.current[Key.E].isPressed)
                turn = Mathf.MoveTowards(turn, maxTurnSpeed, turnSensitivity);
            if (Keyboard.current[Key.Q].isPressed && Keyboard.current[Key.E].isPressed)
                turn = Mathf.MoveTowards(turn, 0, turnSensitivity);
            if (!Keyboard.current[Key.Q].isPressed && !Keyboard.current[Key.E].isPressed)
                turn = Mathf.MoveTowards(turn, 0, turnSensitivity);

            if (isGrounded)
                isFalling = false;

            if ((isGrounded || !isFalling) && jumpSpeed < 1f && Keyboard.current[Key.Space].isPressed)
            {
                jumpSpeed = Mathf.Lerp(jumpSpeed, 1f, 0.5f);
            }
            else if (!isGrounded)
            {
                isFalling = true;
                jumpSpeed = 0;
            }

            if (Keyboard.current[Key.T].wasPressedThisFrame)
            {
                shootRay();
            }
        }

        void FixedUpdate()
        {
            if (!isLocalPlayer || characterController == null || !characterController.enabled)
                return;

            transform.Rotate(0f, turn * Time.fixedDeltaTime, 0f);

            Vector3 direction = new Vector3(horizontal, jumpSpeed, vertical);
            direction = Vector3.ClampMagnitude(direction, 1f);
            direction = transform.TransformDirection(direction);
            direction *= moveSpeed;

            if (jumpSpeed > 0)
                characterController.Move(direction * Time.fixedDeltaTime);
            else
                characterController.SimpleMove(direction);

            isGrounded = characterController.isGrounded;
            velocity = characterController.velocity;
        }


        private void OnGUI()
        {
            if (!isLocalPlayer)
                return;

            GUI.Box(new Rect(Screen.width * 0.5f - 1, Screen.height * 0.5f - 1, 2, 2), GUIContent.none);
        }

        void shootRay()
        {
            RaycastHit hit;
            print("shooting ray");
            Ray ray = gameObject.GetComponentInChildren<Camera>().ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity))
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                Debug.Log("Did Hit");
                Debug.Log("what did I hit? " + hit.transform.gameObject);

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
            objectSpawner.pickupObject();
        }
    }
}