using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ScoreKeeper : NetworkBehaviour
{
    [SerializeField] int startLives;
    [SerializeField] GameObject startPositions;

    Transform[] spawnPositions;

    public List<ScoreCard> alivePlayers = new();

    [HideInInspector] public bool gameOver = false;
    [HideInInspector] public ScoreCard winner;

    public override void OnStartServer()
    {
        base.OnStartServer();
        spawnPositions = startPositions.GetComponentsInChildren<Transform>();
    }


    [Server]
    public void addPlayer(ScoreCard sc)
    {
        alivePlayers.Add(sc);
    }

    [Server]
    public void InitializeScoreCard(ScoreCard sc)
    {
        addPlayer(sc);
        sc.livesLeft = startLives;
    }

    [Server]
    public void RespawnPlayer(PlayerMovement player)
    {
        ScoreCard sc = player.GetComponent<ScoreCard>();
        sc.livesLeft--;

        if (sc.Dead == false)
        {
            int index = Random.Range(1, spawnPositions.Length);
            player.TargetRespawn(spawnPositions[index].position);

        }
        else
        {
            alivePlayers.Remove(sc);
            GameServer.TransitionToSpectator(player.gameObject);
            checkForWinner();
        }
    }

    [Server]
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


}
