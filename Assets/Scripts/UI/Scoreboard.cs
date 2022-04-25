using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Scoreboard : MonoBehaviour
{
    public TextMeshProUGUI livesUI;
    public TextMeshProUGUI scoreboardTextUI;
    public GameObject scoreboard;
    public Image useButton;

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

    // Called by Script Machine component in GamePlayerPrefab -> Input
    public void ShowScoreboard(bool visible)
    {
        scoreboard.SetActive(visible);
    }

    private void Update()
    {
        if(scoreboard.activeInHierarchy)
        {
            scoreboardTextUI.text = GetScoreScreenText();
        }
    }
}
