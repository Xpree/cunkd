using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

public class Spectator : NetworkBehaviour
{
    Camera mainCamera;
    PlayerCameraController currentCamera = null;

    [SyncVar]
    public LobbyClient LobbyClient;

    public int ClientIndex => LobbyClient.Index;
    public string PlayerName => LobbyClient.PlayerName;

    SettingsUI _settings;
    GameInputs _inputs;
    
    private void Start()
    {
        _settings = FindObjectOfType<SettingsUI>(true);
        _inputs = GetComponent<GameInputs>();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        mainCamera = Camera.main;
        var inputs = GetComponent<GameInputs>();
        inputs.SetSpectatorMode();
        inputs.EnableInput();
        FindObjectOfType<SpectatorUI>()?.EnableSpectatorUI();
    }

    public override void OnStopLocalPlayer()
    {
        base.OnStopLocalPlayer();
        FindObjectOfType<SpectatorUI>()?.DisableSpectatorUI();
    }

    void SelectNextCamera()
    {

        if (currentCamera != null)
        {
            currentCamera.DeactivateCamera();
        }


        var playerCameras = FindObjectsOfType<PlayerCameraController>();
        if (currentCamera == null)
        {
            if (playerCameras.Length > 0 && playerCameras[0] != null)
            {
                currentCamera = playerCameras[0];
                currentCamera.ActivateCamera();
                return;
            }
        }

        var previousCamera = currentCamera;
        currentCamera = null;
        for (int i = 0; i < playerCameras.Length; i++)
        {
            if (playerCameras[i] == previousCamera)
            {
                if (i == playerCameras.Length - 1)
                {
                    currentCamera = playerCameras[0];
                }
                else
                {
                    currentCamera = playerCameras[i + 1];
                }
                if (currentCamera != previousCamera)
                {
                    currentCamera.ActivateCamera();
                }
                else
                {
                    currentCamera = null;
                }
                break;
            }
        }

        if (currentCamera == null && mainCamera != null)
            mainCamera.enabled = true;
    }

    public void NextCamera()
    {
        SelectNextCamera();
        var ui = FindObjectOfType<SpectatorUI>();

        if(ui != null)
        {
            if (currentCamera != null)
                ui.SetSpectating(currentCamera.playerTransform.gameObject);
            else
                ui.SetSpectating(null);
        }
    }

    private void Update()
    {
        if (isLocalPlayer)
        {
            if (_inputs.ToggleMenu.triggered)
            {
                _settings.gameObject.SetActive(!_settings.gameObject.activeSelf);
            }

            if (_settings.gameObject.activeSelf)
            {
                _inputs.PreventInput();
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                _inputs.EnableInput();
                Cursor.lockState = CursorLockMode.Locked;
            }

        }
    }
}
