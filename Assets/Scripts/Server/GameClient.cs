using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

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

    public override void OnStartClient()
    {
        base.OnStartClient();
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
        Debug.Log("Game Started!");
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
