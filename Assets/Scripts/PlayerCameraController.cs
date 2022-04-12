using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Mirror;

public class PlayerCameraController : NetworkBehaviour
{
    float mouseSensitivity = 25.0f;
    private Transform cameraTransform;
    public Camera playerCamera;

    Camera mainCamera;   
    float pitch = 0.0f;
    
    // Start is called before the first frame update
    void Awake()
    {
        playerCamera.enabled = false;
        cameraTransform = playerCamera.transform;
        playerCamera.GetComponent<AudioListener>().enabled = false;
    }

    public void ActivateCamera()
    {
        mainCamera = Camera.main;
        mainCamera.enabled = false;
        playerCamera.enabled = true;
        playerCamera.GetComponent<AudioListener>().enabled = true;
    }

    public void DeactivateCamera()
    {
        mainCamera.enabled = true;
        playerCamera.enabled = false;
        playerCamera.GetComponent<AudioListener>().enabled = false;
    }

    public void onMovement(InputAction.CallbackContext context)
    {
        if(!isLocalPlayer) { return; }
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
        ActivateCamera();
    }

    public override void OnStopLocalPlayer()
    {
        DeactivateCamera();
    }
}
