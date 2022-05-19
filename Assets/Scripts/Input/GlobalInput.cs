using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class GlobalInput : MonoBehaviour
{
    private void Start()
    {
        AudioSettings.Singleton.SetMuted(Settings.muted);
        AudioSettings.Singleton.MasterVolumeLevel(Settings.volume);
    }

    void Update()
    {
        if (Keyboard.current.altKey.isPressed && Keyboard.current.enterKey.wasPressedThisFrame)
        {
            Util.ToggleFullscreen();
        }
    }
}
