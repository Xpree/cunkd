using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class LobbyServer : MonoBehaviour
{
    [Header("Room Settings")]

    [SerializeField]
    [Tooltip("This flag controls whether the default UI is shown for the room")]
    public bool ShowRoomGUI = true;

    [SerializeField]
    [Tooltip("Minimum number of players to auto-start the game")]
    public int AutoStartMinPlayers = 1;

    [SerializeField]
    [Tooltip("Prefab to use for the Lobby Client")]
    public LobbyClient LobbyClientPrefab;

    [Scene]
    public string NetworkScene;


    [Header("Diagnostics")]
    /// <summary>
    /// True when all players have submitted a Ready message
    /// </summary>
    [Tooltip("Diagnostic flag indicating all players are ready to play")]
    [SerializeField] bool _allPlayersReady;

    /// <summary>
    /// These slots track players that enter the room.
    /// <para>The slotId on players is global to the game - across all players.</para>
    /// </summary>
    [Tooltip("List of Room Player objects")]
    [SerializeField] LobbyPlayerList _lobbyPlayers = new LobbyPlayerList();

    public bool allPlayersReady
    {
        get => _allPlayersReady;
        set
        {
            bool wasReady = _allPlayersReady;
            bool nowReady = value;

            if (wasReady != nowReady)
            {
                _allPlayersReady = value;
                // all players are readyToBegin, start the game
#if UNITY_SERVER
                StartGame();
#else
                ShowStartButton = true;
#endif
            }
        }
    }

    public bool ShowStartButton = false;
    public bool IsLobbyActive => NetworkManager.IsSceneActive(this.NetworkScene);
    public List<LobbyClient> Players => _lobbyPlayers.clients;

    CunkdNetManager _netManager;

    private void Awake()
    {
        _netManager = GetComponentInParent<CunkdNetManager>();
    }

    void OnValidate()
    {
        // always <= maxConnections
        AutoStartMinPlayers = Mathf.Min(AutoStartMinPlayers, GetComponentInParent<CunkdNetManager>()?.maxConnections ?? int.MaxValue);

        // always >= 0
        AutoStartMinPlayers = Mathf.Max(AutoStartMinPlayers, 0);

        if (LobbyClientPrefab != null)
        {
            NetworkIdentity identity = LobbyClientPrefab.GetComponent<NetworkIdentity>();
            if (identity == null)
            {
                LobbyClientPrefab = null;
                Debug.LogError("RoomPlayer prefab must have a NetworkIdentity component.");
            }
        }
    }

    private void Start()
    {
        if (string.IsNullOrWhiteSpace(NetworkScene))
        {
            Debug.LogError("LobbyServer NetworkScene is empty. Set the NetworkScene in the inspector for the LobbyServer");
            return;
        }
    }

    public void ReadyStatusChanged()
    {
        if (!IsLobbyActive)
            return;

        if (AutoStartMinPlayers <= 0 || _lobbyPlayers.CountReady() >= AutoStartMinPlayers)
        {
            allPlayersReady = true;
        }
        else
        {
            allPlayersReady = false;
        }
    }

    public void ReturnToLobby()
    {
        _netManager.ServerChangeScene(NetworkScene);
        _lobbyPlayers.SetLobbyClientAsActivePlayerObject();
        _lobbyPlayers.ResetLobbyReadyState();
        _lobbyPlayers.RecalculateRoomPlayerIndices();
        allPlayersReady = false;
    }


    public void StartGame()
    {
        _netManager.Game.BeginGame();
    }


    public void AddNewPlayer(NetworkConnectionToClient conn)
    {
        if(conn.clientOwnedObjects.Count == 0)
        {
            var client = Instantiate(LobbyClientPrefab, Vector3.zero, Quaternion.identity);
            _lobbyPlayers.Add(client);
            NetworkServer.AddPlayerForConnection(conn, client.gameObject);
        }
        allPlayersReady = false;
    }

    public void OnDisconnect(NetworkConnectionToClient conn)
    {
        _lobbyPlayers.RemoveConnectedPlayer(conn);
        _lobbyPlayers.ResetLobbyReadyState();
        _allPlayersReady = false;
        if (IsLobbyActive)
        {
            _lobbyPlayers.RecalculateRoomPlayerIndices();
        }
    }

    public void OnClientSceneLoaded(NetworkConnectionToClient conn)
    {
    }

    public void OnServerStarted()
    {
    }

    public void OnServerStopped()
    {
        _lobbyPlayers.Clear();
    }

    void OnGUI()
    {
        if (!ShowRoomGUI)
        {
            return;
        }

        if (!IsLobbyActive)
        {
            if (NetworkServer.active)
            {
                GUILayout.BeginArea(new Rect(Screen.width - 150f, 10f, 140f, 30f));
                if (GUILayout.Button("Return to Room"))
                    this.ReturnToLobby();
                GUILayout.EndArea();
            }
            return;
        }
        else
        {
            GUI.Box(new Rect(10f, 180f, 520f, 150f), "PLAYERS");
            if (allPlayersReady && ShowStartButton && GUI.Button(new Rect(150, 300, 120, 20), "START GAME"))
            {
                // set to false to hide it in the game scene
                ShowStartButton = false;
                this.StartGame();
            }
        }
    }

    [Serializable]
    class LobbyPlayerList
    {
        public List<LobbyClient> clients = new List<LobbyClient>();

        public int Count => clients.Count;

        public void Add(LobbyClient player)
        {
            int index = clients.Count;
            player.Index = index;
            clients.Add(player);
        }

        public void Clear()
        {
            clients.Clear();
        }

        public int CountReady()
        {
            int readyCount = 0;
            foreach (LobbyClient item in clients)
            {
                if (item.ReadyToBegin)
                {
                    ++readyCount;
                }
            }
            return readyCount;
        }

        public void SetLobbyClientAsActivePlayerObject()
        {
            foreach (LobbyClient roomPlayer in clients)
            {
                if (roomPlayer == null)
                    continue;

                // find the game-player object for this connection, and destroy it
                NetworkIdentity identity = roomPlayer.GetComponent<NetworkIdentity>();

                if (NetworkServer.active)
                {
                    // re-add the room object
                    roomPlayer.GetComponent<LobbyClient>().ReadyToBegin = false;
                    NetworkServer.ReplacePlayerForConnection(identity.connectionToClient, roomPlayer.gameObject);
                }
            }
        }

        public void RecalculateRoomPlayerIndices()
        {
            if (clients.Count > 0)
            {
                for (int i = 0; i < clients.Count; i++)
                {
                    clients[i].Index = i;
                }
            }
        }

        public void RemoveConnectedPlayer(NetworkConnectionToClient conn)
        {
            if (conn.identity != null)
            {
                LobbyClient roomPlayer = conn.identity.GetComponent<LobbyClient>();

                if (roomPlayer != null)
                    clients.Remove(roomPlayer);

                foreach (NetworkIdentity clientOwnedObject in conn.clientOwnedObjects)
                {
                    roomPlayer = clientOwnedObject.GetComponent<LobbyClient>();
                    if (roomPlayer != null)
                        clients.Remove(roomPlayer);
                }
            }
        }

        public void ResetLobbyReadyState()
        {
            foreach (LobbyClient player in clients)
            {
                if (player != null)
                    player.GetComponent<LobbyClient>().ReadyToBegin = false;
            }

        }
    }
}