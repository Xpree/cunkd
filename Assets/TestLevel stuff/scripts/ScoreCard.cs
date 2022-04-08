using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class ScoreCard : NetworkBehaviour
{
    [SyncVar] public int index;
    [SyncVar(hook = nameof(UpdateLives))]public int livesLeft;

    ScoreKeeper sk;
    TextMeshProUGUI text;

    [Client]
    void Start()
    {
        text = gameObject.GetComponentsInChildren<TextMeshProUGUI>()[0];
        sk = FindObjectOfType<ScoreKeeper>();
        sk.InitializeScoreCard(this);
    }

    [Client]
    public void UpdateLives(int oldLives, int newLives)
    {
        livesLeft = newLives;
        if (0<livesLeft)
        {
            text.color = Color.green;
            text.text = "Lives: " + newLives;
        }
        else
        {
            text.color = Color.red;
            text.text = "DEAD";
        }
    }

    [ServerCallback]
    public int getLives()
    {
        return livesLeft;
    }

    //[ServerCallback]
    //private void OnTriggerEnter(Collider other)
    //{
    //    //print("ontrigger");
    //    if (other.CompareTag("Respawn"))
    //    {
    //        sk.RespawnPlayer(this.gameObject.GetComponent<FPSPlayerController>());
    //    }
    //}
}
