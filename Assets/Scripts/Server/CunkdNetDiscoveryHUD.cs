using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[DisallowMultipleComponent]
[RequireComponent(typeof(CunkdNetDiscovery))]
public class CunkdNetDiscoveryHUD : MonoBehaviour
{
    readonly Dictionary<long, CunkdServerResponse> discoveredServers = new();
    Vector2 scrollViewPos = Vector2.zero;

    public CunkdNetDiscovery networkDiscovery;

    bool searching = false;

#if UNITY_EDITOR
    void OnValidate()
    {
        if (networkDiscovery == null)
        {
            networkDiscovery = GetComponent<CunkdNetDiscovery>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(networkDiscovery.OnServerFound, OnDiscoveredServer);
            UnityEditor.Undo.RecordObjects(new Object[] { this, networkDiscovery }, "Set NetworkDiscovery");
        }
    }
#endif


    private IEnumerator StartSearch()
    {
        searching = true;
        discoveredServers.Clear();
        networkDiscovery.StartDiscovery();
        yield return new WaitForSeconds(0.5f);
        searching = false;
    }



    void OnGUI()
    {
        if (NetworkManager.singleton == null)
            return;

        if (!NetworkClient.isConnected && !NetworkServer.active && !NetworkClient.active)
            DrawGUI();

        if (NetworkServer.active || NetworkClient.active)
            StopButtons();
    }

    void DrawGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 500));
        GUILayout.BeginHorizontal();


        if (GUILayout.Button("Find Servers"))
        {
            if (!searching)
            {
                StartCoroutine(StartSearch());
            }
        }

        // LAN Host
        if (GUILayout.Button("Start Host"))
        {
            discoveredServers.Clear();
            NetworkManager.singleton.StartHost();
            networkDiscovery.AdvertiseServer();
        }

        // Dedicated server
        if (GUILayout.Button("Start Server"))
        {
            discoveredServers.Clear();
            NetworkManager.singleton.StartServer();
            networkDiscovery.AdvertiseServer();
        }

        GUILayout.EndHorizontal();

        GUILayout.Label("Name:");
        Settings.playerName = GUILayout.TextField(Settings.playerName);

        // show list of found server
        if (searching)
        {
            GUILayout.Label("Searching ...");
        }
        else
        {
            GUILayout.Label($"Discovered Servers [{discoveredServers.Count}]:");

        }

        // servers
        scrollViewPos = GUILayout.BeginScrollView(scrollViewPos);

        foreach (CunkdServerResponse info in discoveredServers.Values)
            if (GUILayout.Button($"[{info.EndPoint.Address}] {info.name}"))
                Connect(info);

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    void StopButtons()
    {
        GUILayout.BeginArea(new Rect(10, 40, 100, 25));

        // stop host if host mode
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            if (GUILayout.Button("Stop Host"))
            {
                NetworkManager.singleton.StopHost();
                networkDiscovery.StopDiscovery();
            }
        }
        // stop client if client-only
        else if (NetworkClient.isConnected)
        {
            if (GUILayout.Button("Stop Client"))
            {
                NetworkManager.singleton.StopClient();
                networkDiscovery.StopDiscovery();
            }
        }
        // stop server if server-only
        else if (NetworkServer.active)
        {
            if (GUILayout.Button("Stop Server"))
            {
                NetworkManager.singleton.StopServer();
                networkDiscovery.StopDiscovery();
            }
        }

        GUILayout.EndArea();
    }

    void Connect(CunkdServerResponse info)
    {
        networkDiscovery.StopDiscovery();
        NetworkManager.singleton.StartClient(info.uri);
    }

    public void OnDiscoveredServer(CunkdServerResponse info)
    {
        // Note that you can check the versioning to decide if you can connect to the server or not using this method
        discoveredServers[info.serverId] = info;
    }
}
