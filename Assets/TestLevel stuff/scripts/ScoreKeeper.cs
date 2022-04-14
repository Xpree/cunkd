using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Mirror;

public class ScoreKeeper : NetworkBehaviour
{
    [SerializeField] int startLives;
    [SerializeField] GameObject startPositions;

    Transform[] spawnPositions;

    public List<ScoreCard> players = new();
    public List<ScoreCard> alivePlayers = new();
    public List<ScoreCard> deadPlayers = new();

    [HideInInspector] public bool gameOver = false;
    [HideInInspector] public ScoreCard winner;

    [HideInInspector] string scoreScreenText;

    [ServerCallback]
    void Start()
    {
        spawnPositions = startPositions.GetComponentsInChildren<Transform>();
    }

    [Server]
    public void addPlayer(ScoreCard sc)
    {
        players.Add(sc);
        alivePlayers.Add(sc);
    }

    [Server]
    public void removePlayer(ScoreCard sc)
    {
        players.Remove(sc);
        alivePlayers.Remove(sc);
        deadPlayers.Remove(sc);
        updatescoreScreenText();
    }

    [Server]
    public void InitializeScoreCard(ScoreCard sc)
    {
        addPlayer(sc);

        sc.livesLeft = startLives;
        sc.index = players.Count;
        sc.playerName = sc.gameObject.GetComponent<PlayerNameTag>().PlayerName;
        updatescoreScreenText();
    }

    [Server]
    public void RespawnPlayer(PlayerMovement player)
    {
        ScoreCard sc = player.GetComponent<ScoreCard>();
        sc.livesLeft--;

        if (0 < sc.getLives())
        {
            int index = Random.Range(1, spawnPositions.Length);
            player.TargetRespawn(spawnPositions[index].position);

        }
        else
        {
            sc.dead = true;
            alivePlayers.Remove(sc);
            deadPlayers.Add(sc);
            GameServer.TransitionToSpectator(player.gameObject);
            checkForWinner();
        }
        updatescoreScreenText();
    }

    [ServerCallback]
    private void checkForWinner()
    {
        if (GameServer.Instance.HasRoundStarted && alivePlayers.Count == 1)
        {
            winner = alivePlayers[0];
            gameOver = true;
            LobbyServer.Instance.SetWinner(LobbyClient.FromConnection(winner.connectionToClient).PlayerName);
            GameServer.EndGame();
        }
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement player = other.gameObject.GetComponent<PlayerMovement>();
        if (player)
        {
            RespawnPlayer(player);
        }
        else
        {
            NetworkServer.Destroy(other.gameObject);
        }
    }


    [ServerCallback]
    public void updatescoreScreenText()
    {
        scoreScreenText = "Player:\t\tLives:\n";
        foreach (ScoreCard player in alivePlayers)
        {
            scoreScreenText += (player.playerName + "\t\t" + player.livesLeft + "\n");
        }
        foreach (ScoreCard player in deadPlayers)
        {
            scoreScreenText += (player.playerName + "\t\tDEAD\n");
        }
        foreach (ScoreCard player in players)
        {
            player.scoreScreenText = scoreScreenText;
        }
    }
}
