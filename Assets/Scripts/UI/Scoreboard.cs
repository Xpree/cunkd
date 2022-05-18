using UnityEngine;

public class Scoreboard : MonoBehaviour
{
    public GameObject scoreScreen;
    public UILives localLives;

    private void Start()
    {
        localLives.gameObject.SetActive(false);
    }

    public void SetLocalLives(int lives)
    {
        if (lives > 0)
        {
            localLives.gameObject.SetActive(true);
            localLives.SetLives(lives);
        }
        else
        {
            localLives.gameObject.SetActive(false);
        }
    }    

    // Called by Script Machine component in GamePlayerPrefab -> Input
    public void ShowScoreboard(bool visible)
    {
        scoreScreen.SetActive(visible);
    }
}
