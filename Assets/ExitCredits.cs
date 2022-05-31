using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitCredits : MonoBehaviour
{
    void Update()
    {
     
        if(UnityEngine.InputSystem.Keyboard.current.anyKey.wasPressedThisFrame || UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame ||
            UnityEngine.InputSystem.Mouse.current.rightButton.wasPressedThisFrame)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("OfflineScene");
        }
    }
}
