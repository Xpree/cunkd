using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScoreboardItem : MonoBehaviour
{

    [SerializeField] TMPro.TMP_Text playerName;

    [SerializeField] UILives livesUI;
    [SerializeField] GameObject spectatingUI;
    [SerializeField] GameObject playingUI;

    public void SetPlayerName(string name)
    {
        playerName.text = name;
    }

    public void SetLives(int lives)
    {
        livesUI.SetLives(lives);
    }

    public void SetSpectating()
    {
        playingUI.SetActive(false);
        spectatingUI.SetActive(true);
    }

    public void SetPlaying()
    {
        playingUI.SetActive(true);
        spectatingUI.SetActive(false);
    }    
}
