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

    GameInputs _inputs;

    [SyncVar]
    float syncedPitch;
    
    // Start is called before the first frame update
    void Awake()
    {
        playerCamera.enabled = false;
        cameraTransform = playerCamera.transform;
        playerCamera.GetComponent<AudioListener>().enabled = false;
    }

    public override void OnStartLocalPlayer()
    {
        ActivateCamera();
        Cursor.lockState = CursorLockMode.Locked;
        _inputs = FindObjectOfType<GameInputs>();
    }

    public override void OnStopLocalPlayer()
    {
        DeactivateCamera();
    }

    [Command]
    void CmdUpdatePitch(float pitch)
    {
        syncedPitch = pitch;
    }

    private void Update()
    {
        if (!isLocalPlayer) {
            cameraTransform.localRotation = Quaternion.Euler(syncedPitch, 0.0f, 0.0f);
            return;
        }
        
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        moveCamera(_inputs.Look);
    }
    
    void moveCamera(Vector2 delta)
    {        
        float xMovement = delta.x * mouseSensitivity * Time.deltaTime;
        float yMovement = delta.y * mouseSensitivity * Time.deltaTime;

        pitch -= yMovement;
        pitch = Mathf.Clamp(pitch, -90.0f, 90.0f);

        CmdUpdatePitch(pitch);
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0.0f, 0.0f);
        transform.Rotate(Vector3.up * xMovement);
    }

    public void ActivateCamera()
    {
        mainCamera = Camera.main;
        if (mainCamera)
            mainCamera.enabled = false;
        playerCamera.enabled = true;
        playerCamera.GetComponent<AudioListener>().enabled = true;
    }

    public void DeactivateCamera()
    {
        if (mainCamera)
            mainCamera.enabled = true;
        playerCamera.enabled = false;
        playerCamera.GetComponent<AudioListener>().enabled = false;
    }


}
