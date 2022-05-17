using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class UILobby : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text mapNameText;
    [SerializeField] UILobbyPlayerItem[] players;
    bool? previouslyReady = null;

    [SerializeField] GameObject hostUI;
    [SerializeField] GameObject clientUI;

    private void Start()
    {
        var isServer = NetworkServer.active;
        hostUI.SetActive(isServer);
        clientUI.SetActive(!isServer);
    }

    public void SetMapName(string name)
    {
        mapNameText.text = name;
    }

    public void UpdatePlayers()
    {

        var clients = FindObjectsOfType<LobbyClient>();
        for (int i = 0; i < players.Length; i++)
        {
            players[i].SetWaiting();
        }

        for (int i = 0; i < clients.Length; i++)
        {
            var c = clients[i];
            players[c.Index].SetPlayer(c);
        }
    }

    public void ToggleLocalPlayerReady()
    {
        // Guard against spamming ready
        if (previouslyReady == null || previouslyReady.Value != LobbyClient.Local.ReadyToBegin)
        {
            previouslyReady = LobbyClient.Local.ReadyToBegin;
            LobbyClient.Local.CmdChangeReadyState(!LobbyClient.Local.ReadyToBegin);
        }
    }

    public void SelectNextMap()
    {
        GameServer.SelectNextMap();
        LobbyServer.Instance.UpdateSelectedMap();
    }

    public void StartGame()
    {
        GameServer.BeginGame();
    }

    public void Disconnect()
    {
        if(NetworkManager.singleton == null)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            return;
        }
        
        if(NetworkServer.active)
        {
            NetworkManager.singleton.StopHost();
        }
        else
        {
            NetworkManager.singleton.StopClient();
        }
    }
}
