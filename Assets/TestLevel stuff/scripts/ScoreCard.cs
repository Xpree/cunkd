using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class ScoreCard : NetworkBehaviour
{
    [SyncVar] public int index;
    [SyncVar]int livesLeft;

    [SyncVar] int itemsPickedUp;

    ScoreKeeper sk;
    [SyncVar] string stringText; 
    TextMeshProUGUI text;

    // Start is called before the first frame update
    [ServerCallback]
    void Start()
    {
        sk = FindObjectOfType<ScoreKeeper>();
        text = gameObject.GetComponentsInChildren<TextMeshProUGUI>()[1];
        sk.InitializeScoreCard(this);
    }

    [ServerCallback]
    public void UpdateLives(int lives)
    {
        //print("updating lives");
        livesLeft = lives;
        if (0<livesLeft)
        {
            text.color = Color.green;
            stringText = "Lives: " + lives;
            text.text = "Lives: " + lives;
        }
        else
        {
            text.color = Color.red;
            stringText = "Lives: " + lives;
            text.text = "DEAD";
        }
    }

    [ServerCallback]
    public int getLives()
    {
        return livesLeft;
    }
    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        //print("ontrigger");
        if (other.CompareTag("Respawn"))
        {
            sk.RespawnPlayer(this.gameObject.GetComponent<FPSPlayerController>());
        }
    }
}
