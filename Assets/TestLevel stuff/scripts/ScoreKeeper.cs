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
            sc.UpdateLives(startLives);
            sc.index = index++;
        }

        [ServerCallback]
        public void RespawnPlayer(FPSPlayerController player)
        {
            ScoreCard sc = player.GetComponent<ScoreCard>();
            sc.UpdateLives(sc.getLives()-1);

            if (0 < sc.getLives())
            {
                int index = Random.Range(0, spawnPositions.Length);
                player.TRpcSetPosition(spawnPositions[index].position);
                //sc.gameObject.transform.position = spawnPositions[index].position;
            }
            else
            {
                print("no more lives left");
            }
        }
    }
}
