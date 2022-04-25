using UnityEngine;
using Mirror;
using NetworkRigidbody = Mirror.Experimental.NetworkRigidbody;

public class PhysicsDebugHelper : NetworkBehaviour
{        
    [Command(requiresAuthority = false)]
    void SpawnPlayer(NetworkIdentity atPosition)
    {
        var position = atPosition?.transform.position ?? Vector3.zero;
        var rotation = atPosition?.transform.rotation ?? Quaternion.identity;

        var gamePlayer = Instantiate(GameServer.Instance.PlayerPrefab, position, rotation);

        gamePlayer.GetComponent<NetworkTransform>().clientAuthority = false;
        gamePlayer.GetComponent<NetworkRigidbody>().clientAuthority = false;
        gamePlayer.GetComponent<NetworkTransformChild>().clientAuthority = false;

        NetworkServer.Spawn(gamePlayer.gameObject);
    }

    void DoDebugWindow(int windowID)
    {
        if (GUILayout.Button("Spawn GamePlayerPrefab"))
        {
            SpawnPlayer(NetworkClient.localPlayer);
        }
    }

    Rect debugWindowRect = new Rect(20, 200, 120, 50);
    private void OnGUI()
    {
        if(Cursor.lockState == CursorLockMode.None)
            debugWindowRect = GUILayout.Window(0, debugWindowRect, DoDebugWindow, "Physics Debug");
    }
}
