using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AdaptiveMusic : MonoBehaviour
{    
    NetworkManager networkManager;

    public float maxPlayersLeft = 8;
    public float minPlayersLeft = 2;

    // Start is called before the first frame update
    void Start()
    {
        networkManager = FindObjectOfType<NetworkManager>();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(networkManager.numPlayers);
    }
}
