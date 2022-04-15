using UnityEngine;

public class AutoSpawnNetworkManager : MonoBehaviour
{
    public CunkdNetManager NetworkManagerPrefab;

    void Start()
    {
        if(FindObjectOfType<CunkdNetManager>() == null)
        {
            Instantiate(NetworkManagerPrefab).StartHost();
        }            
    }
}
