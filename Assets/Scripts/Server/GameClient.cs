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

    [SyncVar]
    public bool IsCunkd;

    Coroutine lastCunkd;
        
    System.Collections.IEnumerator SetCunkdCoroutine(float duration)
    {
        IsCunkd = true;
        var timer = NetworkTimer.FromNow(duration);
        while(timer.HasTicked == false)
        {
            yield return null;
        }
        IsCunkd = false;
        lastCunkd = null;
    }
    
    [Server]
    public void SetCunkd(float duration)
    {
        if(lastCunkd != null)
        {
            StopCoroutine(lastCunkd);
        }
        lastCunkd = StartCoroutine(SetCunkdCoroutine(duration));
    }

    [SyncVar(hook = nameof(OnChangeInvul)]
    public bool IsInvulnerable;

    Coroutine lastInvulnerable;

    void UpdateLayer()
    {
        if (IsInvulnerable)
        {
            this.gameObject.layer = LayerMask.NameToLayer("InvulnerablePlayer");
        }
        else
        {
            this.gameObject.layer = LayerMask.NameToLayer("Player");
        }
    }
    
    void OnChangeInvul(bool previous, bool current)
    {
        UpdateLayer();
    }

    System.Collections.IEnumerator SetInvulnerableCoroutine(float duration)
    {
        IsInvulnerable = true;
        UpdateLayer();
        var timer = NetworkTimer.FromNow(duration);
        while (timer.HasTicked == false)
        {
            yield return null;
        }
        IsInvulnerable = false;
        UpdateLayer();
        lastInvulnerable = null;
    }

    [Server]
    public void SetInvulnerable(float duration)
    {
        if (lastInvulnerable != null)
        {
            StopCoroutine(lastInvulnerable);
        }
        lastInvulnerable = StartCoroutine(SetInvulnerableCoroutine(duration));
    }

    private void Awake()
    {
        CameraController = GetComponentInChildren<PlayerCameraController>(true);
        _inputs = GetComponentInChildren<GameInputs>(true);
    }

    private void Start()
    {
        UpdateLayer();
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
