using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoSpawnNetworkManager : MonoBehaviour
{
    public CunkdNetManager NetworkManagerPrefab;

    void Start()
    {
        if(CunkdNetManager.Instance == null)
        {
            CunkdNetManager.HostCurrentScene(NetworkManagerPrefab);
        }            
    }
}
