using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

public class GameClient : NetworkBehaviour
{
    bool _loaded = false;

    public bool Loaded { get { return _loaded; } }

    [Server]
    public LobbyClient GetLobbyClient()
    {
        return LobbyClient.FromConnection(this.connectionToClient);
    }

    [Command]
    void CmdLoaded()
    {
        this._loaded = true;
        GameServer.OnGameClientLoaded();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        CmdLoaded();
        this._loaded = true;
    }

    IEnumerator DelayGameStart(double networkTime)
    {
        while (networkTime > NetworkTime.time)
        {
            //double remaining = networkTime - NetworkTime.time;
            yield return null;
        }

        var input = GetComponent<PlayerInput>();

        if (input != null)
        {
            input.enabled = true;
        }
        else
        {
            Debug.Log("Missing input component!");
        }

    }


    [TargetRpc]
    public void TargetGameStart(double networkTime)
    {
        StartCoroutine(DelayGameStart(networkTime));
    }


    private void OnGUI()
    {
        if (!isLocalPlayer)
            return;
    }
}
