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
    [SyncVar] public NetworkTimer RoundEnded;

    public bool IsRoundActive => RoundStart.TickTime > RoundEnded.TickTime;


    public static GameStats Singleton;

    public static NetworkTimer RoundTimer => Singleton.RoundStart;

    public static bool IsRoundStarted => Singleton.IsRoundActive;

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

    void OnGUI()
    {
        CunkdNetManager cunkd = NetworkManager.singleton as CunkdNetManager;
        if (cunkd && cunkd.Lobby.ShowRoomGUI && cunkd.Lobby.IsLobbyActive)
        {
            if (string.IsNullOrEmpty(LastGameWinner) == false)
            {
                GUILayout.Label($"Last game winner: {LastGameWinner}!");
            }
        }
    }
}
