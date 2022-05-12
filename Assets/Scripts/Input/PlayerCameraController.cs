using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using System;

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
    public GameObject shakeCollider;

    public List<CameraShake> activeShakers = new();
    public bool zoomed = false;
    public float currentFieldOfView => zoomed ? zoomfov : Settings.cameraFov;

    [HideInInspector]
    public NetworkIdentity playerIdentity;

    void Awake()
    {
        playerCamera.enabled = false;
        cameraTransform = playerCamera.transform;
        cameraPosition = cameraTransform.localPosition;
    }

    private void Start()
    {
        playerIdentity = playerTransform.GetComponent<NetworkIdentity>();
    }

    private void OnEnable()
    {
        if(shakeCollider != null)
        {
            EventBus.Register(new EventHook(nameof(EventCameraShake), shakeCollider), new Action<CameraShakeSource>(OnCameraShake));
        }
    }
    private void OnDisable()
    {
        if (shakeCollider != null)
        {
            EventBus.Unregister(new EventHook(nameof(EventCameraShake), shakeCollider), new Action<CameraShakeSource>(OnCameraShake));
        }
    }

    private void OnCameraShake(CameraShakeSource source)
    {
        this.AddShake(source);
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
        playerTransform.Rotate(Vector3.up * xMovement);
        UpdateCamera(FetchCameraShake());
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
        sample.Rotation = Quaternion.identity;

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
            sample.FieldOfView += shake.FieldOfView;
        }
        
        return sample;
    }

    const float MaximumFOV = 100.0f;

    void UpdateCamera(ShakeSample shake)
    {
        playerCamera.fieldOfView = Mathf.Clamp(currentFieldOfView + shake.FieldOfView, zoomfov * 0.5f, MaximumFOV);
        var rotationEuler = shake.Rotation.eulerAngles;
        rotationEuler.x += pitch;
        rotationEuler.x = Mathf.Clamp(rotationEuler.x, -89.9f, 89.9f);
        cameraTransform.localRotation = Quaternion.Euler(rotationEuler);
        cameraTransform.localPosition = cameraPosition + shake.Position;
    }

    private void Update()
    {
        var shake = FetchCameraShake();
        if (playerIdentity.HasControl())
        {
            UpdateCamera(shake);
        }
    }
    
    public void AddShake(CameraShakeSource source)
    {
        activeShakers.Add(source.cameraShake);
    }

}
