using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UILobbyPlayerItem : MonoBehaviour
{
    [SerializeField] TMP_Text playerName;
    [SerializeField] GameObject readyMark;
    [SerializeField] GameObject waitingMark;
    [SerializeField] GameObject notReadyMark;

    void Awake()
    {
        SetWaiting();
    }

    public void SetPlayerName(string name)
    {
        playerName.text = name;
        playerName.gameObject.SetActive(true);
        waitingMark.SetActive(false);
    }

    public void SetWaiting()
    {
        playerName.gameObject.SetActive(false);
        readyMark.SetActive(false);
        notReadyMark.SetActive(false);
        waitingMark.SetActive(true);
    }

    public void SetPlayerReady(bool ready)
    {
        readyMark.SetActive(ready);
        notReadyMark.SetActive(!ready);
    }

    public void SetPlayer(LobbyClient client)
    {
        SetPlayerName(client.PlayerName);
        SetPlayerReady(client.ReadyToBegin);
    }
}
