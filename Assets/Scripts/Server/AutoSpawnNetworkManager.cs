using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoSpawnNetworkManager : MonoBehaviour
{
    public CunkdNetManager NetworkManagerPrefab;

    void Start()
    {
        if(CunkdNetManager.Instance == null)
        {
            var cunkd = Instantiate(NetworkManagerPrefab);
            cunkd.AutoHostAndPlay = SceneManager.GetActiveScene().name;
            Debug.Log("Autohosting: " + cunkd.AutoHostAndPlay);
        }            
    }
}
