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
    [SyncVar(hook = nameof(OnPlayerReadyChange))]
    public bool ReadyToBegin;

    /// <summary>
    /// Diagnostic index of the player, e.g. Player1, Player2, etc.
    /// </summary>
    [Tooltip("Diagnostic index of the player, e.g. Player1, Player2, etc.")]
    [SyncVar]
    public int Index;

    [SyncVar(hook = nameof(OnPlayerNameChange))]
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
        UILobby.Singleton?.UpdatePlayers();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        CmdChangePlayerName(Settings.playerName);
        Local = this;
    }

    private void Start()
    {
        UILobby.Singleton?.UpdatePlayers();
    }

    private void OnDestroy()
    {
        UILobby.Singleton?.UpdatePlayers();
    }

    void OnPlayerNameChange(string previous, string current)
    {
        UILobby.Singleton?.UpdatePlayers();
    }

    void OnPlayerReadyChange(bool previus, bool current)
    {
        UILobby.Singleton?.UpdatePlayers();
    }

    [TargetRpc]
    public void TargetRpcCurrentMap(string map) 
    {
        UILobby.Singleton?.SetMapName(map);
    }

}
