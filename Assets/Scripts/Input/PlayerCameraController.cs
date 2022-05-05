using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class PlayerCameraController : MonoBehaviour
{
    public Transform playerTransform;
    public Transform cameraTransform;
    public Camera playerCamera;
    public float zoomfov;
    
    Camera mainCamera;
    float pitch = 0.0f;

    public UnityEvent OnCameraActivated;
    public UnityEvent OnCameraDeactivated;

    public Vector3 cameraPosition;

    public List<CameraShake> activeShakers = new();
    public bool zoomed = false;
    public float currentFieldOfView => zoomed ? zoomfov : Settings.cameraFov;

    void Awake()
    {
        playerCamera.enabled = false;
        cameraTransform = playerCamera.transform;

        cameraPosition = cameraTransform.localPosition;
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
        pitch = Mathf.Clamp(pitch, -89.9f, 89.9f);
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0, 0);
        playerTransform.Rotate(Vector3.up * xMovement);
    }

    static void EnableCamera(Camera camera, bool enable)
    {
        if (camera == null)
            return;
        camera.enabled = enable;
        var listener = camera.GetComponent<FMODUnity.StudioListener>();
        if (listener != null)
            listener.enabled = enable;
    }


    public void ActivateCamera()
    {
        mainCamera = Camera.main;
        EnableCamera(mainCamera, false);
        EnableCamera(playerCamera, true);
        OnCameraActivated?.Invoke();
        Cursor.lockState = CursorLockMode.Locked;
        playerCamera.fieldOfView = currentFieldOfView;
        EventBus.Trigger(nameof(EventPlayerCameraActivated), playerTransform.gameObject);
    }

    public void DeactivateCamera()
    {
        EnableCamera(mainCamera, true);
        EnableCamera(playerCamera, false);
        OnCameraDeactivated?.Invoke();
        Cursor.lockState = CursorLockMode.None;

        EventBus.Trigger(nameof(EventPlayerCameraDeactivated), playerTransform.gameObject);
    }

    public void ToggleZoom()
    {
        zoomed = !zoomed;
        playerCamera.fieldOfView = currentFieldOfView;
    }

    public void ZoomOff()
    {
        zoomed = false;
        playerCamera.fieldOfView = currentFieldOfView;
    }

    public bool IsCameraActive => playerCamera.enabled;

    ShakeSample FetchCameraShake()
    {
        ShakeSample sample = new();

        for (int i = activeShakers.Count - 1; i >= 0; --i)
        {
            var shaker = activeShakers[i];
            if (!shaker.IsActive)
            {
                activeShakers.RemoveAt(i);
                continue;
            }
            var shake = shaker.Sample();
            sample.Position += shake.Position;
            sample.Rotation *= shake.Rotation;
            sample.FOV += shake.FOV;
        }
        
        return sample;
    }

    /*
    private void Update()
    {
        var shake = FetchCameraShake();
        playerCamera.fieldOfView = Mathf.Max(currentFieldOfView + shake.FOV, zoomfov * 0.5f);
        var rotationEuler = shake.Rotation.eulerAngles;
        rotationEuler.x += pitch;
        rotationEuler.x = Mathf.Clamp(rotationEuler.x, -89.9f, 89.9f);
        cameraTransform.localRotation = Quaternion.Euler(rotationEuler);
        cameraTransform.localPosition = cameraPosition + shake.Position;
    }*/
    
    public void AddShake(CameraShake shaker)
    {
        activeShakers.Add(shaker);
    }

}
