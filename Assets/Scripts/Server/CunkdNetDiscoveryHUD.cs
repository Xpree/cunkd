using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEngine.Events;

[DisallowMultipleComponent]
[RequireComponent(typeof(CunkdNetDiscovery))]
public class CunkdNetDiscoveryHUD : MonoBehaviour
{
    public readonly Dictionary<long, CunkdServerResponse> discoveredServers = new();

    public CunkdNetDiscovery networkDiscovery;

    public UnityEvent onServersUpdated;
    public int generateFakeServers = 0;


#if UNITY_EDITOR
    void OnValidate()
    {
        if (networkDiscovery == null)
        {
            networkDiscovery = GetComponent<CunkdNetDiscovery>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(networkDiscovery.OnServerFound, OnDiscoveredServer);
            UnityEditor.Undo.RecordObjects(new UnityEngine.Object[] { this, networkDiscovery }, "Set NetworkDiscovery");
        }
    }
#endif

    public void OnDiscoveredServer(CunkdServerResponse info)
    {
        Debug.Log("OnDiscoveredServer");
        // Note that you can check the versioning to decide if you can connect to the server or not using this method
        discoveredServers[info.serverId] = info;
        onServersUpdated.Invoke();
    }

    public void StartServer()
    {
        discoveredServers.Clear();
        networkDiscovery.StopDiscovery();
        NetworkManager.singleton.StartServer();
        networkDiscovery.AdvertiseServer();
    }

    public void StartHost()
    {
        networkDiscovery.StopDiscovery();
        discoveredServers.Clear();
        NetworkManager.singleton.StartHost();
        networkDiscovery.AdvertiseServer();
    }

    public void JoinHost(Uri uri)
    {
        networkDiscovery.StopDiscovery();
        NetworkManager.singleton.StartClient(uri);
    }

    public void StopDiscovery()
    {
        networkDiscovery.StopDiscovery();
        discoveredServers.Clear();
    }

    public void StartDiscovery()
    {
        discoveredServers.Clear();
        if (generateFakeServers > 0)
        {
            for (int i = 0; i < generateFakeServers; ++i)
            {
                discoveredServers.Add(i, new CunkdServerResponse { name = "Server " + i, serverId = i });
            }
            onServersUpdated.Invoke();
        }        
        networkDiscovery.StartDiscovery();        
    }

}
