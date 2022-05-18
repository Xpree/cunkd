using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScoreScreen : MonoBehaviour
{
    public UIScoreboardItem[] players;

    void Update()
    {
        var alivePlayers = FindObjectsOfType<ScoreCard>();
        System.Array.Sort(alivePlayers);

        for (int i = 0; i < alivePlayers.Length; i++)
        {
            players[i].gameObject.SetActive(true);
            players[i].SetPlayerName(alivePlayers[i].PlayerName);
            players[i].SetLives(alivePlayers[i].livesLeft);
            players[i].SetPlaying();
        }

        int n = alivePlayers.Length;
        var deadPlayers = FindObjectsOfType<Spectator>();
        for (int i = 0; i < deadPlayers.Length; i++)
        {
            players[n + i].gameObject.SetActive(true);
            players[n + i].SetPlayerName(deadPlayers[i].PlayerName);
            players[n + i].SetSpectating();
        }
        
        for (int i = n + deadPlayers.Length; i < players.Length; i++)
        {
            players[i].gameObject.SetActive(false);
        }
    }
}
