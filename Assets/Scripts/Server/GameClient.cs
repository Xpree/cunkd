using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class GameClient : NetworkBehaviour
{
    bool _loaded = false;

    public bool Loaded { get { return _loaded; } }

    GameInputs _inputs = null;

    [SyncVar]
    public LobbyClient LobbyClient;
    public int ClientIndex => LobbyClient?.Index ?? -1;
    public string PlayerName => LobbyClient?.PlayerName ?? "[DISCONNECTED]";

    public PlayerCameraController CameraController;


    private void Awake()
    {
        CameraController = GetComponentInChildren<PlayerCameraController>(true);
        _inputs = GetComponentInChildren<GameInputs>(true);
    }


    [Server]
    public LobbyClient GetLobbyClient()
    {
        return LobbyClient.FromConnection(this.connectionToClient);
    }

    [Command]
    void CmdLoaded()
    {
        this._loaded = true;
        if(GameServer.Instance.HasRoundStarted)
        {
            GameServer.TransitionToSpectator(this.gameObject);
        }
        else
        {
            GameServer.OnGameClientLoaded();
        }        
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        CmdLoaded();
        _loaded = true;

        _inputs.gameObject.SetActive(true);
        _inputs.PreventInput();
        CameraController.ActivateCamera();
    }


    public override void OnStopLocalPlayer()
    {
        CameraController.DeactivateCamera();
    }


    IEnumerator DelayGameStart(NetworkTimer networkTime)
    {
        while (networkTime.HasTicked == false)
        {
            yield return null;
        }

        _inputs.SetPlayerMode();
        _inputs.EnableInput();
        GetComponent<PlayerMovement>().SetKinematicOff();
    }


    [TargetRpc]
    public void TargetGameStart(NetworkTimer roundStart)
    {
        FindObjectOfType<Countdown>()?.StartCountdown(roundStart);
        StartCoroutine(DelayGameStart(roundStart));
    }


    public void LogDebug(string text)
    {
        Debug.Log($"[Player: {this.PlayerName}] {text}");
    }
}
