using UnityEngine;
using Mirror;
using UnityEngine.Events;

public class PlayerCameraController : NetworkBehaviour
{
    float mouseSensitivity = 25.0f;
    private Transform cameraTransform;
    public Camera playerCamera;

    Camera mainCamera;   
    float pitch = 0.0f;
    [SyncVar]
    float syncedPitch;

    GameInputs _inputs;

    public UnityEvent OnCameraActivated;
    public UnityEvent OnCameraDeactivated;
    
    void Awake()
    {
        playerCamera.enabled = false;
        cameraTransform = playerCamera.transform;
        playerCamera.GetComponent<AudioListener>().enabled = false;
    }

    public override void OnStartLocalPlayer()
    {
        ActivateCamera();
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
        
        if(Cursor.lockState == CursorLockMode.Locked)
            moveCamera(_inputs.Look.ReadValue<Vector2>());
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

    static void EnableCamera(Camera camera, bool enable)
    {
        if (camera == null)
            return;
        camera.enabled = enable;
        var audioListener = camera.GetComponent<AudioListener>();
        if(audioListener != null)
            audioListener.enabled = enable;
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
