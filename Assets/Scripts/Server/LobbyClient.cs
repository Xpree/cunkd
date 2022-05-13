using UnityEngine;
using Mirror;
using System;

/// <summary>
/// This component works in conjunction with the NetworkRoomManager to make up the multiplayer room system.
/// <para>The RoomPrefab object of the NetworkRoomManager must have this component on it. This component holds basic room player data required for the room to function. Game specific data for room players can be put in other components on the RoomPrefab or in scripts derived from NetworkRoomPlayer.</para>
/// </summary>
[DisallowMultipleComponent]
public class LobbyClient : NetworkBehaviour
{
    [Header("Diagnostics")]
    /// <summary>
    /// Diagnostic flag indicating whether this player is ready for the game to begin.
    /// <para>Invoke CmdChangeReadyState method on the client to set this flag.</para>
    /// <para>When all players are ready to begin, the game will start. This should not be set directly, CmdChangeReadyState should be called on the client to set it on the server.</para>
    /// </summary>
    [Tooltip("Diagnostic flag indicating whether this player is ready for the game to begin")]
    [SyncVar]
    public bool ReadyToBegin;

    /// <summary>
    /// Diagnostic index of the player, e.g. Player1, Player2, etc.
    /// </summary>
    [Tooltip("Diagnostic index of the player, e.g. Player1, Player2, etc.")]
    [SyncVar]
    public int Index;

    [SyncVar]
    string _playerName;

    public string PlayerName => string.IsNullOrEmpty(_playerName) ? $"Player {Index+1}" : _playerName;


    public static LobbyClient Local = null;

    public static LobbyClient FromConnection(NetworkConnectionToClient conn)
    {
        if (conn == null)
            return null;

        foreach (var obj in conn.clientOwnedObjects)
        {
            var client = obj.GetComponent<LobbyClient>();
            if(client != null)
            {
                return client;
            }
        }

        return null;
    }

    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }


    [Command]
    public void CmdChangeReadyState(bool readyState)
    {
        ReadyToBegin = readyState;
        CunkdNetManager cunkd = NetworkManager.singleton as CunkdNetManager;
        if (cunkd != null)
        {
            cunkd.Lobby.ReadyStatusChanged();
        }
    }

    [Command]
    public void CmdChangePlayerName(string name)
    {
        _playerName = name;
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        CmdChangePlayerName(Settings.playerName);
        this._playerName = Settings.playerName;
        Local = this;
    }

    void OnGUI()
    {
        
        CunkdNetManager cunkd = NetworkManager.singleton as CunkdNetManager;
        if (cunkd && cunkd.Lobby.ShowRoomGUI && cunkd.Lobby.IsLobbyActive)
        {
            Cursor.lockState = CursorLockMode.None;
            DrawPlayerReadyState();
            DrawPlayerReadyButton();
        }
    }

    void DrawPlayerReadyState()
    {
        GUILayout.BeginArea(new Rect(20f + (Index * 100), 200f, 90f, 130f));

        GUILayout.Label($"[{PlayerName}]");

        if (ReadyToBegin)
            GUILayout.Label("Ready");
        else
            GUILayout.Label("Not Ready");

        if (((isServer && Index > 0) || isServerOnly) && GUILayout.Button("REMOVE"))
        {
            // This button only shows on the Host for all players other than the Host
            // Host and Players can't remove themselves (stop the client instead)
            // Host can kick a Player this way.
            GetComponent<NetworkIdentity>().connectionToClient.Disconnect();
        }

        GUILayout.EndArea();
    }

    void DrawPlayerReadyButton()
    {
        if (NetworkClient.active && isLocalPlayer)
        {
            GUILayout.BeginArea(new Rect(20f, 300f, 120f, 20f));

            if (ReadyToBegin)
            {
                if (GUILayout.Button("Cancel"))
                    CmdChangeReadyState(false);
            }
            else
            {
                if (GUILayout.Button("Ready"))
                    CmdChangeReadyState(true);
            }

            GUILayout.EndArea();
        }
    }

}
