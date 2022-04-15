using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;
public class PlayerNameTag : NetworkBehaviour
{
    public TextMeshProUGUI NameCanvas;
    [SyncVar(hook = nameof(OnPlayerNameChange))]
    public string PlayerName = "undefined";

    public void OnPlayerNameChange(string previous, string current)
    {
        NameCanvas.text = current;

    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        PlayerName = LobbyClient.FromConnection(this.connectionToClient).PlayerName;
    }
}
