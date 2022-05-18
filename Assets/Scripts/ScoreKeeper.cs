using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ScoreKeeper : NetworkBehaviour
{
    [SerializeField] int startLives;
    [SerializeField] GameObject startPositions;
    [SerializeField] LayerMask destroyOnCollision;

    Transform[] spawnPositions;

    public List<ScoreCard> alivePlayers = new();

    [HideInInspector] public bool gameOver = false;
    [HideInInspector] public ScoreCard winner;       

    public override void OnStartServer()
    {
        base.OnStartServer();
        spawnPositions = startPositions.GetComponentsInChildren<Transform>();
        //FindObjectOfType<AdaptiveMusic>().scoreKeeper = this;        
    }

    public void setPlayerSpawnPositions(GameObject positions)
    {
        spawnPositions = positions.GetComponentsInChildren<Transform>();
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
            var spawn = spawnPositions[index];
            GameServer.PurgeOwnedObjects(player.gameObject);
            player.TargetRespawn(spawn.position, spawn.rotation);
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
            GameServer.Stats.ShowWinner(winner.PlayerName);
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
        else if(other.gameObject.GetComponent<NetworkIdentity>() != null && destroyOnCollision == (destroyOnCollision | (1 << other.gameObject.layer)))
        {
            NetworkServer.Destroy(other.gameObject);
        }
        else
        {
            NetworkIdentity obj = other.transform.root.gameObject.GetComponentInChildren<NetworkIdentity>();
            if (obj && destroyOnCollision == (destroyOnCollision | (1 << obj.gameObject.layer)) && !obj.gameObject.GetComponent<PlayerMovement>())
            {
                NetworkServer.Destroy(obj.gameObject);
            }
        }
    }


}
