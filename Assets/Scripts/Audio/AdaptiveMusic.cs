using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AdaptiveMusic : NetworkBehaviour
{    
    NetworkManager networkManager;

    [HideInInspector]
    public ScoreKeeper scoreKeeper;
    
    private float playersLeft;
        
    public override void OnStartServer()
    {
        networkManager = FindObjectOfType<NetworkManager>();        
    }
        
    void Update()
    {           
        if (scoreKeeper != null)
        {            
            if (scoreKeeper.alivePlayers.Count >= 8)
            {
                playersLeft = 8;
            }
            else if (scoreKeeper.alivePlayers.Count <= 2)
            {
                playersLeft = 2;
            }
            else
            {
                playersLeft = scoreKeeper.alivePlayers.Count;
            }            

            GetComponent<FMODUnity.StudioEventEmitter>().SetParameter("Players Left", playersLeft);
        }        
    }    
}
