using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

public class GameClient : NetworkBehaviour
{
    bool _loaded = false;

    public bool Loaded { get { return _loaded; } }

    GameInputs _inputs = null;

    [SyncVar]
    public LobbyClient LobbyClient;


    public int ClientIndex => LobbyClient.Index;
    public string PlayerName => LobbyClient.PlayerName;


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

        _inputs = FindObjectOfType<GameInputs>();
        _inputs.SetPlayerMode();
        _inputs.PreventInput();
    }

    IEnumerator DelayGameStart(double networkTime)
    {
        while (networkTime > NetworkTime.time)
        {
            //double remaining = networkTime - NetworkTime.time;
            yield return null;
        }

        _inputs.EnableInput();
    }


    [TargetRpc]
    public void TargetGameStart(double networkTime)
    {
        FindObjectOfType<Countdown>()?.StartCountdown(networkTime);
        StartCoroutine(DelayGameStart(networkTime));
    }
}
