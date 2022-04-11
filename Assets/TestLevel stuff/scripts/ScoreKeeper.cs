using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Mirror;

namespace Mirror
{
    public class ScoreKeeper : NetworkBehaviour
    {
        [SerializeField] int startLives;
        [SerializeField] GameObject startPositions;

        Transform[] spawnPositions;

        [HideInInspector] public List<ScoreCard> players = new();
        [HideInInspector] public List<ScoreCard> alivePlayers = new();
        [HideInInspector] public List<ScoreCard> deadPlayers = new();

        [HideInInspector] public bool gameOver = false;
        [HideInInspector] public ScoreCard winner;

        [HideInInspector] string scoreScreenText;

        int index =0;

        [ServerCallback]
        void Start()
        {
            spawnPositions = startPositions.GetComponentsInChildren<Transform>();
        }

        [ServerCallback]
        public void InitializeScoreCard(ScoreCard sc)
        {
            print("init scorecard");
            sc.livesLeft = startLives;
            players.Add(sc);
            sc.index = players.Count;
            sc.playerName = sc.gameObject.GetComponent<PlayerNameTag>().PlayerName;
            alivePlayers.Add(sc);
            updatescoreScreenText();
        }

        [ServerCallback]
        public void RespawnPlayer(FPSPlayerController player)
        {
            ScoreCard sc = player.GetComponent<ScoreCard>();
            sc.livesLeft--;

            if (0 < sc.getLives())
            {
                int index = Random.Range(1, spawnPositions.Length);
                player.GetComponent<Rigidbody>().velocity = new Vector3(0,0,0);
                player.TRpcSetPosition(spawnPositions[index].position);

            }
            else
            {
                sc.dead = true;
                alivePlayers.Remove(sc);
                deadPlayers.Add(sc);
                checkForWinner();
            }
            updatescoreScreenText();
        }

        [ServerCallback]
        private void checkForWinner()
        {
            if (alivePlayers.Count == 1)
            {
                winner = alivePlayers[0];
                gameOver = true;
                reloadScene();
            }
        }

        [ServerCallback]
        void reloadScene()
        {
            //print("reloading scene");
            //NetworkManager.singleton.ServerChangeScene(SceneManager.GetActiveScene().name);
        }

        [ServerCallback]
        private void OnTriggerEnter(Collider other)
        {
            FPSPlayerController player = other.gameObject.GetComponent<FPSPlayerController>();
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
}
