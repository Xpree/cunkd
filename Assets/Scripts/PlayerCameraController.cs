using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Mirror;

public class PlayerCameraController : NetworkBehaviour
{

    float mouseSensitivity = 50.0f;
    private Transform cameraTransform;
    private Camera playerCamera;

    float pitch = 0.0f;

    // Start is called before the first frame update
    void Awake()
    {
        playerCamera = Camera.main;
        cameraTransform = playerCamera.transform;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void onMovement(InputAction.CallbackContext context)
    {
        Vector2 position = context.ReadValue<Vector2>();

        float xMovement = position.x * mouseSensitivity * Time.deltaTime;
        float yMovement = position.y * mouseSensitivity * Time.deltaTime;

        pitch -= yMovement;
        pitch = Mathf.Clamp(pitch, -90.0f, 90.0f);
      
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0.0f, 0.0f);
        transform.Rotate(Vector3.up * xMovement);
    }

    public override void OnStartLocalPlayer()
    {
        if (playerCamera != null)
        {
            // configure and make camera a child of player with 3rd person offset
            playerCamera.orthographic = false;
            playerCamera.transform.SetParent(transform);
            playerCamera.transform.localPosition = new Vector3(0f, 3f, -8f);
            playerCamera.transform.localEulerAngles = new Vector3(10f, 0f, 0f);
        }
    }

    public override void OnStopLocalPlayer()
    {
        if (playerCamera != null)
        {
            playerCamera.transform.SetParent(null);
            SceneManager.MoveGameObjectToScene(playerCamera.gameObject.transform.parent.gameObject, SceneManager.GetActiveScene());
            playerCamera.orthographic = true;
            playerCamera.transform.localPosition = new Vector3(0f, 70f, 0f);
            playerCamera.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
        }
    }
}
