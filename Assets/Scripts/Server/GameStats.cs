using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// GameStats persists through scene loads. Use GameServer.Stats to grab them once a connection/host has been made.
/// </summary>
public class GameStats : NetworkBehaviour
{
    [SyncVar] public string LastGameWinner;

    [SyncVar] public NetworkTimer RoundStart;
    [SyncVar(hook = nameof(OnRoundEnded))] public NetworkTimer RoundEnded;

    public bool IsRoundActive => RoundStart.TickTime > RoundEnded.TickTime;


    public static GameStats Singleton;

    public static float RoundTimer => Singleton.RoundStart.IsSet ? (float)Singleton.RoundStart.Elapsed : 0;

    public static bool IsRoundStarted => Singleton.IsRoundActive;
    public static bool HasRoundEnded => Singleton.RoundEnded.TickTime > Singleton.RoundStart.TickTime;

    void OnRoundEnded(NetworkTimer previous, NetworkTimer current)
    {
        if(current.IsSet)
        {
            FindObjectOfType<Countdown>()?.StopCountdown();
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(this);
        Singleton = this;
    }

    public void OnDestroy()
    {
        if (Singleton == this)
            Singleton = null;
    }

    [ClientRpc]
    void RpcShowWinner(string winner)
    {
        FindObjectOfType<UIGameResult>().SetWinner(winner);
    }

    [Server]
    public void ShowWinner(string winner)
    {
        LastGameWinner = winner;
        RpcShowWinner(winner);
    }


    [ClientRpc]
    void RpcShowEndedByHost()
    {
        FindObjectOfType<UIGameResult>().SetEndedByHost();
    }
    
    [Server]
    public void ShowEndedByHost()
    {
        RpcShowEndedByHost();
    }

}
