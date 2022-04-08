using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace Mirror
{
    public class ScoreKeeper : NetworkBehaviour
    {
        [SerializeField] int startLives;
        [SerializeField] GameObject startPositions;
        Transform[] spawnPositions;

        int index =0;

        [ServerCallback]
        void Start()
        {
            spawnPositions = startPositions.GetComponentsInChildren<Transform>();
        }

        [ServerCallback]
        public void InitializeScoreCard(ScoreCard sc)
        {
            sc.livesLeft = startLives;
            sc.index = index++;
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
                print("no more lives left");
            }
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
    }
}
