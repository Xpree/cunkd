using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class ScoreCard : NetworkBehaviour
{
    [SyncVar] public int index;
    [SyncVar(hook = nameof(UpdateLives))]public int livesLeft;
    [SyncVar] public bool dead;

    ScoreKeeper sk;
    TextMeshProUGUI text;

    [Client]
    void Awake()
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
            dead = true;
            text.color = Color.red;
            text.text = "DEAD";
        }
    }

    [ServerCallback]
    public int getLives()
    {
        return livesLeft;
    }
}
