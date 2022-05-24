using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AdaptiveMusic : NetworkBehaviour
{   
    public static AdaptiveMusic singleton;
    private int lowLife;

    private void Awake()
    {
        singleton = this;
    }
    
    
    public void UpdateLives(int lives) 
    {
        if (lives >= 3)
        {
            lowLife = 0;
        }
        else
        {
            lowLife = 1;
        }
    }    
        
    void Update()
    {
        

        GetComponent<FMODUnity.StudioEventEmitter>().SetParameter("LowLife", lowLife);
                        
    }    
}
