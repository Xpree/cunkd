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

    public string LocalPlayerName;

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

    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        base.OnServerReady(conn);
        _lobbyServer.OnClientReady(conn);
        if (!_lobbyServer.IsLobbyActive)
        {
            _gameServer.OnClientReady(conn);
        }
    }



    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 40, 215, 9999));
        if (!NetworkClient.isConnected && !NetworkServer.active)
        {
            StartButtons();
        }
        else
        {
            StatusLabels();
        }

        // client ready
        if (NetworkClient.isConnected && !NetworkClient.ready)
        {
            if (GUILayout.Button("Client Ready"))
            {
                NetworkClient.Ready();
                if (NetworkClient.localPlayer == null)
                {
                    NetworkClient.AddPlayer();
                }
            }
        }

        StopButtons();

        GUILayout.EndArea();
    }

    void StartButtons()
    {
        if (!NetworkClient.active)
        {
            LocalPlayerName = GUILayout.TextField(LocalPlayerName);

            // Server + Client
            if (Application.platform != RuntimePlatform.WebGLPlayer)
            {
                if (GUILayout.Button("Host (Server + Client)"))
                {
                    this.StartHost();
                }
            }

            // Client + IP
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Client"))
            {
                this.StartClient();
            }
            // This updates networkAddress every frame from the TextField
            this.networkAddress = GUILayout.TextField(this.networkAddress);
            GUILayout.EndHorizontal();

            // Server Only
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                // cant be a server in webgl build
                GUILayout.Box("(  WebGL cannot be server  )");
            }
            else
            {
                if (GUILayout.Button("Server Only")) this.StartServer();
            }
        }
        else
        {
            // Connecting
            GUILayout.Label($"Connecting to {this.networkAddress}..");
            if (GUILayout.Button("Cancel Connection Attempt"))
            {
                this.StopClient();
            }
        }
    }

    void StatusLabels()
    {
        // host mode
        // display separately because this always confused people:
        //   Server: ...
        //   Client: ...
        if (NetworkServer.active && NetworkClient.active)
        {
            GUILayout.Label($"<b>Host</b>: running via {Transport.activeTransport}");
        }
        // server only
        else if (NetworkServer.active)
        {
            GUILayout.Label($"<b>Server</b>: running via {Transport.activeTransport}");
        }
        // client only
        else if (NetworkClient.isConnected)
        {
            GUILayout.Label($"<b>Client</b>: connected to {this.networkAddress} via {Transport.activeTransport}");
        }
    }

    void StopButtons()
    {
        // stop host if host mode
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            if (GUILayout.Button("Stop Host"))
            {
                this.StopHost();
            }
        }
        // stop client if client-only
        else if (NetworkClient.isConnected)
        {
            if (GUILayout.Button("Stop Client"))
            {
                this.StopClient();
            }
        }
        // stop server if server-only
        else if (NetworkServer.active)
        {
            if (GUILayout.Button("Stop Server"))
            {
                this.StopServer();
            }
        }
    }
}
