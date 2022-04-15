using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Scoreboard : MonoBehaviour
{
    GameInputs gameInputs;

    public TextMeshProUGUI livesUI;
    public TextMeshProUGUI scoreboardTextUI;
    public GameObject scoreboard;

    private void Start()
    {
        gameInputs = FindObjectOfType<GameInputs>();
    }

    public void SetLocalLives(int lives)
    {
        if (lives <= 0)
            livesUI.text = "";
        else
            livesUI.text = "Lives: " + lives;
    }    

    static string GetScoreScreenText()
    {
        var alivePlayers = FindObjectsOfType<ScoreCard>();
        System.Array.Sort(alivePlayers);

        var scoreScreenText = "Player:\t\tLives:\n";
        foreach (ScoreCard player in alivePlayers)
        {
            scoreScreenText += (player.PlayerName + "\t\t" + player.livesLeft + "\n");
        }

        var deadPlayers = FindObjectsOfType<Spectator>();
        foreach (var player in deadPlayers)
        {
            scoreScreenText += (player.PlayerName + "\t\tDEAD\n");
        }
        return scoreScreenText;
    }

    private void Update()
    {
        if (gameInputs.ShowScoreboard.WasPressedThisFrame() || gameInputs.ShowScoreboard.WasReleasedThisFrame())
        {
            bool activateScoreboard = gameInputs.ShowScoreboard.WasPressedThisFrame();
            if (activateScoreboard)
            {
                scoreboardTextUI.text = GetScoreScreenText();
            }
            scoreboard.SetActive(activateScoreboard);
        }
    }
}
