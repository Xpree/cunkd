using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGameResult : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text winnerName;
    [SerializeField] GameObject winnerUI;
    [SerializeField] GameObject endByHost;
    
    public void SetWinner(string name)
    {
        winnerName.text = name;
        winnerUI.SetActive(true);
    }

    public void SetEndedByHost()
    {
        endByHost.SetActive(true);
    }
}
