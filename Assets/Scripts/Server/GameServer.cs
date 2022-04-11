using UnityEngine;
using Mirror;

public class GameServer : MonoBehaviour
{
    [Scene]
    public string NetworkScene;

    public NetworkIdentity PlayerPrefab;

    CunkdNetManager _netManager;

    private void Awake()
    {
        _netManager = GetComponentInParent<CunkdNetManager>();
    }

    private void Start()
    {
        if (string.IsNullOrWhiteSpace(NetworkScene))
        {
            Debug.LogError("GameServer NetworkScene is empty. Set the NetworkScene in the inspector for the GameServer");
        }
    }

    public void BeginGame()
    {
        _netManager.ServerChangeScene(this.NetworkScene);
    }

    public void AddNewPlayer(NetworkConnectionToClient conn)
    {
        SpawnGamePlayer(conn);
    }

    void SpawnGamePlayer(NetworkConnectionToClient conn)
    {
        Transform startPos = _netManager.GetStartPosition();
        Vector3 position = startPos?.position ?? Vector3.zero;
        Quaternion rotation = startPos?.rotation ?? Quaternion.identity;

        var gamePlayer = Instantiate(this.PlayerPrefab, position, rotation);

        var client = LobbyClient.FromConnection(conn);
        gamePlayer.GetComponent<PlayerNameTag>().PlayerName = client.PlayerName;
        
        NetworkServer.ReplacePlayerForConnection(conn, gamePlayer.gameObject, true);
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


    }

    public void OnServerStopped()
    {        
    }

    public void OnServerSceneLoaded(string sceneName)
    {
        if (sceneName == this.NetworkScene) {

            foreach (var client in _netManager.Lobby.Players)
            {
                var conn = client?.connectionToClient;
                if (conn != null)
                {
                    SpawnGamePlayer(conn);
                }
            }
        }
    }

}
