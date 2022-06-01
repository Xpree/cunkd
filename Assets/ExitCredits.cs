using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitCredits : MonoBehaviour
{
    void Update()
    {
     
        if(UnityEngine.InputSystem.Keyboard.current.anyKey.wasReleasedThisFrame || UnityEngine.InputSystem.Mouse.current.leftButton.wasReleasedThisFrame ||
            UnityEngine.InputSystem.Mouse.current.rightButton.wasReleasedThisFrame)
        {
            if(CunkdNetManager.Instance != null)
                SceneManager.MoveGameObjectToScene(CunkdNetManager.Instance.gameObject, SceneManager.GetActiveScene());
            UnityEngine.SceneManagement.SceneManager.LoadScene("OfflineScene");
        }
    }
}
