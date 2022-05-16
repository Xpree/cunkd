using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Mirror;

public class UIServerItem : MonoBehaviour
{

    public TextMeshProUGUI serverName;
    public TextMeshProUGUI serverCounts;


    System.Uri serverUri;

    public void SetServer(CunkdServerResponse info, int index)
    {
        var rectTransform = GetComponent<RectTransform>();

        var pos = rectTransform.localPosition;
        pos.y = index * -64;
        rectTransform.localPosition = pos;

        serverName.text = info.name;
        serverCounts.text = info.currentPlayers + "/" + info.maxPlayers;
        serverUri = info.uri;
    }

    public void JoinGame()
    {
        NetworkManager.singleton.StartClient(serverUri);
    }
}
