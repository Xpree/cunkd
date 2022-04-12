using UnityEngine;
using Mirror;

/*
 * Network Manager Execution order:
 * https://mirror-networking.gitbook.io/docs/faq/execution-order
 * 
 * Network Manager initialization:
 * void Awake() - Grab components off GameObject
 * void Start() - Pre server/client start code
 * void ConfigureHeadlessFrameRate() // Server only
 * void OnStartServer() // Server only
 * void OnStartHost() // Host only
 * 
 * Network Manager connection:
 * void OnStartClient() // Client only - called when starting connection
 * void OnClientConnect() // Client only - called after scene loaded, tells server the connection is ready and requests AddPlayer
 * void OnClientDisconnect() // Client only - called when disconnected
 * void OnStopClient() // Client only - called when StopClient has been called
 * 
 * void OnServerConnect() // Server only - client connection accepted
 * void OnServerReady(NetworkConnectionToClient conn) // Server only - called when client has loaded scene and is ready
 * void OnServerAddPlayer(NetworkConnectionToClient conn) // Server only - called when client has loaded scene, is ready and auto create player is ticked
 * 
 * void OnServerDisconnect(NetworkConnectionToClient conn) // Server only - removes player object when client disconnects by default
 */
public class CunkdNetManager : NetworkManager
{
    LobbyServer _lobbyServer;
    GameServer _gameServer;

    public LobbyServer Lobby => _lobbyServer;
    public GameServer Game => _gameServer;

    [Scene]
    public string AutoHostAndPlay;


    public static CunkdNetManager Instance => NetworkManager.singleton as CunkdNetManager;

    public override void Awake()
    {
        _lobbyServer = GetComponentInChildren<LobbyServer>(true);
        _gameServer = GetComponentInChildren<GameServer>(true);
        this.dontDestroyOnLoad = true;
        base.Awake();
    }

    public override void Start()
    {
        base.Start();
        if (!string.IsNullOrEmpty(AutoHostAndPlay) && Application.isEditor)
        {
            this.onlineScene = AutoHostAndPlay;
            this.StartHost();
        }
    }

    /// <summary>
    /// This is invoked when a server is started - including when a host is started.
    /// <para>StartServer has multiple signatures, but they all cause this hook to be called.</para>
    /// </summary>
    public override void OnStartServer()
    {
        _lobbyServer.OnServerStarted();
        _gameServer.OnServerStarted();
    }

    public override void OnStopServer()
    {
        _lobbyServer.OnServerStopped();
        _gameServer.OnServerStopped();
    }

    /// <summary>
    /// Called on the server when a client adds a new player with NetworkClient.AddPlayer.
    /// <para>The default implementation for this function creates a new player object from the playerPrefab.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        _lobbyServer.AddNewPlayer(conn);
        if (!_lobbyServer.IsLobbyActive)
        {
            _gameServer.AddNewPlayer(conn);
        }
    }


    /// <summary>
    /// Called on the server when a client disconnects.
    /// </summary>
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        _lobbyServer.OnDisconnect(conn);
        _gameServer.OnDisconnect(conn);
        base.OnServerDisconnect(conn);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        _gameServer.OnServerSceneLoaded(sceneName);
        base.OnServerSceneChanged(sceneName);
    }
}
