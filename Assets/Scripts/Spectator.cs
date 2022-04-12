using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class Spectator : NetworkBehaviour
{
    Camera mainCamera;
    PlayerCameraController currentCamera = null;
        
    public override void OnStartLocalPlayer()
    {
        mainCamera = Camera.main;
        base.OnStartLocalPlayer();
        FindObjectOfType<SpectatorUI>()?.EnableSpectatorUI();
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
    }

    [ClientCallback]
    private void Update()
    {
        if (!isLocalPlayer)
            return;

        if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            NextCamera();
        }
    }
}
