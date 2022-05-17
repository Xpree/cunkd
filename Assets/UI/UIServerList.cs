using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIServerList : MonoBehaviour
{
    public GameObject serverItemPrefab;
    public GameObject serverList;
    public CunkdNetDiscoveryHUD discoveryHUD;
    public int serverListItemSize = 64;
    
    private void OnEnable()
    {
        discoveryHUD.onServersUpdated.AddListener(UpdateList);
        UpdateList();
    }

    private void OnDisable()
    {
        discoveryHUD.onServersUpdated.RemoveListener(UpdateList);
    }

    bool dirty = false;
    public void UpdateList()
    {
        dirty = true;
    }

    private void Update()
    {
        if (!dirty)
            return;
        dirty = false;

        var discoveredServers = discoveryHUD.discoveredServers;
        int n = discoveredServers.Count - serverList.transform.childCount;
        for (int i = 0; i < n; i++)
        {
            Instantiate(serverItemPrefab, serverList.transform);
        }

        int index = 0;
        foreach (var server in discoveredServers)
        {
            serverList.transform.GetChild(index).GetComponent<UIServerItem>().SetServer(server.Value, index);
            ++index;
        }

        var content = serverList.GetComponent<RectTransform>();
        var sizeDelta = content.sizeDelta;

        sizeDelta.y = index * serverListItemSize;
        content.sizeDelta = sizeDelta;


        int c = serverList.transform.childCount;
        for (int i = discoveredServers.Count; i < c; i++)
        {
            Destroy(serverList.transform.GetChild(i).gameObject);
        }
    }
}
