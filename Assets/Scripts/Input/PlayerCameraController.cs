using UnityEngine;
using Mirror;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerCameraController : MonoBehaviour
{
    public Transform playerTransform;
    public Transform cameraTransform;
    public Camera playerCamera;

    Camera mainCamera;   
    float pitch = 0.0f;

    public UnityEvent OnCameraActivated;
    public UnityEvent OnCameraDeactivated;
    
    void Awake()
    {
        playerCamera.enabled = false;
        cameraTransform = playerCamera.transform;        
    }

    public void OnCameraInput(InputAction.CallbackContext ctx)
    {
        if (Cursor.lockState == CursorLockMode.Locked)
            MoveCamera(ctx.ReadValue<Vector2>());
    }

    public void MoveCamera(Vector2 delta)
    {
        float xMovement = delta.x * Settings.mouseSensitivityYaw * Time.deltaTime;
        float yMovement = delta.y * Settings.mouseSensitivityPitch * Time.deltaTime;

        pitch -= yMovement;
        pitch = Mathf.Clamp(pitch, -90.0f, 90.0f);
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0.0f, 0.0f);
        playerTransform.Rotate(Vector3.up * xMovement);
    }

    static void EnableCamera(Camera camera, bool enable)
    {
        if (camera == null)
            return;
        camera.enabled = enable;
        var listener = camera.GetComponent<FMODUnity.StudioListener>();
        if(listener != null)
            listener.enabled = enable;
    }


    public void ActivateCamera()
    {
        mainCamera = Camera.main;
        EnableCamera(mainCamera, false);
        EnableCamera(playerCamera, true);
        OnCameraActivated?.Invoke();
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void DeactivateCamera()
    {
        EnableCamera(mainCamera, true);
        EnableCamera(playerCamera, false);
        OnCameraDeactivated?.Invoke();
        Cursor.lockState = CursorLockMode.None;
    }
}
