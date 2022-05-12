using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

/// <summary>
/// Everything in this class is for the server-side only.
/// </summary>
public class GameServer : MonoBehaviour
{
    /// <summary>
    /// The singleton instance of the GameServer
    /// </summary>
    public static GameServer Instance => CunkdNetManager.Instance.Game;

    public static GameServer GetInstance() => GameServer.Instance;

    [SerializeField] GameStats GameStatsPrefab;

    public static NetworkTimer startTime;

    /// <summary>
    /// The list of maps available
    /// </summary>
    [Scene]
    [SerializeField] string[] NetworkScene;
    [SerializeField] public int SelectedScene;

    [Tooltip("The player avatar prefab")]
    [SerializeField] public GameClient PlayerPrefab;

    [Tooltip("The spectator prefab")]
    [SerializeField] public Spectator SpectatorPrefab;

    [Tooltip("The delay of the start of the map in seconds")]
    [SerializeField] public double DelayStart = 4;

    [SerializeField] public GameSettings Settings;

    /// <summary>
    /// The NetworkTime.time when the round starts
    /// </summary>
    [HideInInspector]
    public double RoundStart = 0;

    /// <summary>
    /// The list of players in the game.
    /// </summary>
    [HideInInspector]
    public List<GameClient> Players = new();

    [HideInInspector]
    public List<Spectator> Spectators = new();

    public bool HasRoundStarted => RoundStart >= DelayStart && RoundStart <= NetworkTime.time;

    GameStats _gameStats;
    public static GameStats Stats => Instance?._gameStats;

    private void Start()
    {
        if (NetworkScene.Length == 0 || string.IsNullOrWhiteSpace(NetworkScene[0]))
        {
            Debug.LogError("GameServer NetworkScene is empty. Set the NetworkScene in the inspector for the GameServer");
        }
    }

    public static void SelectNextMap()
    {
        var self = CunkdNetManager.Instance.Game;

        self.SelectedScene += 1;
        if (self.SelectedScene >= self.NetworkScene.Length)
            self.SelectedScene = 0;
    }

    public static string SelectMapName() {
        var self = CunkdNetManager.Instance.Game;
        return System.IO.Path.GetFileNameWithoutExtension(self.NetworkScene[self.SelectedScene]);
    }

    public static void BeginGame()
    {
        startTime = NetworkTimer.Now;
        var self = CunkdNetManager.Instance.Game;
        CunkdNetManager.Instance.ServerChangeScene(self.NetworkScene[self.SelectedScene]);
    }

    public static void EndGame()
    {
        var netManager = CunkdNetManager.Instance;
        var self = netManager.Game;

        self.Players.Clear();
        self.Spectators.Clear();
        self.RoundStart = 0;

        netManager.Lobby.ReturnToLobby();
    }

    public void AddNewPlayer(NetworkConnectionToClient conn)
    {
        TryAddPlayer(conn);
    }

    void SpawnSpectator(NetworkConnectionToClient conn)
    {
        var spectator = Instantiate(this.SpectatorPrefab);
        spectator.LobbyClient = LobbyClient.FromConnection(conn);
        NetworkServer.ReplacePlayerForConnection(conn, spectator.gameObject, true);
        Spectators.Add(spectator);
    }

    void SpawnGamePlayer(NetworkConnectionToClient conn)
    {
        Transform startPos = CunkdNetManager.Instance.GetStartPosition();
        Vector3 position = startPos?.position ?? Vector3.zero;
        Quaternion rotation = startPos?.rotation ?? Quaternion.identity;

        var gamePlayer = Instantiate(this.PlayerPrefab, position, rotation);
        gamePlayer.LobbyClient = LobbyClient.FromConnection(conn);
        NetworkServer.ReplacePlayerForConnection(conn, gamePlayer.gameObject, true);
        Players.Add(gamePlayer);
    }

    void TryAddPlayer(NetworkConnectionToClient conn)
    {
        if (conn == null || !conn.isReady)
            return;

        if (!HasRoundStarted)
        {
            if (conn?.identity?.GetComponent<GameClient>() == null)
            {
                SpawnGamePlayer(conn);
            }
        }
        else
        {
            if (conn?.identity?.GetComponent<Spectator>() == null)
                SpawnSpectator(conn);
        }
    }

    void SpawnAllLobbyPlayers()
    {
        var lobbyPlayers = CunkdNetManager.Instance.Lobby.Players;
        foreach (var client in lobbyPlayers)
        {
            TryAddPlayer(client?.connectionToClient);
        }
    }

    public static void TransitionToSpectator(GameObject player)
    {
        GameServer.PurgeOwnedObjects(player);
        var conn = player?.GetComponent<NetworkIdentity>()?.connectionToClient;
        if (conn != null)
        {
            Instance.SpawnSpectator(conn);
        }
        NetworkServer.Destroy(player);
    }

    public void OnDisconnect(NetworkConnectionToClient conn)
    {
#if UNITY_SERVER
        if (_netManager.numPlayers < 1 && !_netManager.Lobby.IsLobbyActive)
            _netManager.Lobby.ReturnToLobby();
#endif
    }

    public void OnServerStarted()
    {
        _gameStats = Instantiate(this.GameStatsPrefab);
        NetworkServer.Spawn(_gameStats.gameObject);
    }

    public void OnServerStopped()
    {
    }

    public void OnServerSceneLoaded(string sceneName)
    {
        if (sceneName != LobbyServer.Instance.NetworkScene)
        {
            SpawnAllLobbyPlayers();
        }
    }

    public void OnClientReady(NetworkConnectionToClient conn)
    {
        TryAddPlayer(conn);
    }

    public bool IsPlayersLoaded()
    {
        var lobbyPlayers = CunkdNetManager.Instance.Lobby.Players;
        foreach (var client in lobbyPlayers)
        {
            var conn = client?.connectionToClient;
            if (conn != null && !conn.isReady)
            {
                return false;
            }
        }

        foreach (var client in Players)
        {
            if ((client?.Loaded ?? true) == false)
            {
                return false;
            }
        }
        return true;
    }

    public void StartRound()
    {
        RoundStart = NetworkTime.time + this.DelayStart;
        foreach (var client in Players)
        {
            client?.TargetGameStart(RoundStart);
        }
    }

    public static void OnGameClientLoaded()
    {
        var network = CunkdNetManager.Instance;
        var self = network.Game;

        if (self.HasRoundStarted)
        {
            return;
        }

        if (self.IsPlayersLoaded())
        {
            self.StartRound();
        }
    }

    public static void PurgeOwnedObjects(NetworkConnectionToClient player)
    {
        if (player != null)
        {
            var list = new List<NetworkIdentity>(player.clientOwnedObjects);
            foreach (var item in list)
            {
                if (item.GetComponent<GameClient>() != null ||
                    item.GetComponent<LobbyClient>() != null ||
                    item.GetComponent<Spectator>() != null)
                {
                    continue;
                }

                NetworkServer.Destroy(item.gameObject);
            }
        }
    }

    public static void PurgeOwnedObjects(GameObject player)
    {
        if (player == null)
            return;

        var conn = player?.GetComponent<NetworkIdentity>()?.connectionToClient;
        PurgeOwnedObjects(conn);
    }

    public static void Respawn(GameObject client)
    {
        var spawn = CunkdNetManager.Instance.GetStartPosition();
        var player = client?.GetComponent<PlayerMovement>();
        if(player != null)
        {
            player.TargetRespawn(spawn.position, spawn.rotation);
        }
        else
        {
            Debug.LogError("Attempted to respawn a client without PlayerMovement component.");
        }        
    }
}
