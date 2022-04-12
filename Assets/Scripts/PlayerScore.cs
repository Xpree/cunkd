using UnityEngine;
using Mirror;
using Mirror.Experimental;

public class PlayerScore : NetworkBehaviour
{
    [SyncVar]
    public int index;

    [SerializeField] public int startLives;

    [SyncVar]
    [SerializeField] public int livesLeft;



    void OnGUI()
    {
        //GUI.Box(new Rect(10f + (index * 110), 10f, 100f, 25f), $"P{index}: {score:0000000}");
    }


}
