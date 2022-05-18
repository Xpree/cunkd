using UnityEngine;

public class Scoreboard : MonoBehaviour
{
    public GameObject scoreScreen;

    // Called by Script Machine component in GamePlayerPrefab -> Input
    public void ShowScoreboard(bool visible)
    {
        scoreScreen.SetActive(visible);
    }
}
