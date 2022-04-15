using UnityEngine;
using UnityEngine.InputSystem;

public class GlobalInput : MonoBehaviour
{
    public void ToggleFullscreen()
    {
        var res = Screen.currentResolution;
        if (Screen.fullScreen)
        {
            // Toggle back to 16 by 9 landscape at 75% height of the screen                
            var h = (res.height >> 2) * 3;
            var w = h * 16 / 9;
            Screen.SetResolution(w, h, FullScreenMode.Windowed, res.refreshRate);

        }
        else
        {
            Screen.SetResolution(res.width, res.height, Settings.windowedFullscreenMode ? FullScreenMode.FullScreenWindow : FullScreenMode.ExclusiveFullScreen, res.refreshRate);
        }
    }


    void Update()
    {
        if(Keyboard.current.altKey.isPressed && Keyboard.current.enterKey.wasPressedThisFrame)
        {
            ToggleFullscreen();
        }
    }
}
