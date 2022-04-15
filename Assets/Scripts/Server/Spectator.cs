using UnityEngine;
using Mirror;

public class Spectator : NetworkBehaviour
{
    Camera mainCamera;
    PlayerCameraController currentCamera = null;
    GameInputs gameInputs;

    [SyncVar]
    public LobbyClient LobbyClient;

    public int ClientIndex => LobbyClient.Index;
    public string PlayerName => LobbyClient.PlayerName;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        mainCamera = Camera.main;
        FindObjectOfType<SpectatorUI>()?.EnableSpectatorUI();
        gameInputs = FindObjectOfType<GameInputs>();
        gameInputs.SetSpectatorMode();
    }

    public override void OnStopLocalPlayer()
    {
        base.OnStopLocalPlayer();
        FindObjectOfType<SpectatorUI>()?.DisableSpectatorUI();
    }

    void NextCamera()
    {
        if(currentCamera != null)
        {
            currentCamera.DeactivateCamera();
        }

 
        var playerCameras = FindObjectsOfType<PlayerCameraController>();        
        if(currentCamera == null)
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

        if (currentCamera == null)
            mainCamera.enabled = true;
    }

    [ClientCallback]
    private void Update()
    {
        if (!isLocalPlayer)
            return;

        if(gameInputs.SpectateNext.WasPerformedThisFrame())
        {
            NextCamera();
        }
    }
}
