using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.InputSystem;

public class ScoreCard : NetworkBehaviour
{
    [SyncVar] public int index;
    [SyncVar(hook = nameof(UpdateLives))]public int livesLeft;
    [HideInInspector][SyncVar] public bool dead;
    [HideInInspector] [SyncVar] public string playerName = "playerName";
    [HideInInspector] [SyncVar(hook = nameof(updateScoreScreenText))] public string scoreScreenText;

    [SerializeField]public GameObject scoreScreen;

    ScoreKeeper sk;
    TextMeshProUGUI text;

    [Client]
    void Start()
    {
        text = gameObject.GetComponentsInChildren<TextMeshProUGUI>()[0];
        sk = FindObjectOfType<ScoreKeeper>();
        sk.InitializeScoreCard(this);
    }

    [ServerCallback]
    private void OnDestroy()
    {
        sk.removePlayer(this);
    }

    [Client]
    public void UpdateLives(int oldLives, int newLives)
    {
        updateScoreCard();
        if (isLocalPlayer)
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
    }

    [Command]
    void updateScoreCard()
    {
        sk.updatescoreScreenText();
    }

    [Client]
    public void updateScoreScreenText(string oldText, string newText)
    {
        scoreScreen.GetComponentInChildren<TextMeshProUGUI>().text = newText;
    }

    [ServerCallback]
    public int getLives()
    {
        return livesLeft;
    }

    [Client]
    private void Update()
    {
        if (Keyboard.current[Key.Tab].wasPressedThisFrame)
        {
            scoreScreen.SetActive(true);
        }
        if (Keyboard.current[Key.Tab].wasReleasedThisFrame)
        {
            scoreScreen.SetActive(false);
        }
    }
}