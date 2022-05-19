using UnityEngine;

public static class Settings
{
    public static bool muted
    {
        get { return PlayerPrefs.GetInt("Muted", 0) != 0; }
        set { PlayerPrefs.SetInt("Muted", value ? 1 : 0); }
    }

    public static float volume
    {
        get { return Mathf.Clamp01(PlayerPrefs.GetFloat("Volume", 0.5f)); }        
        set { PlayerPrefs.SetFloat("Volume", value); }        
    }


    public static float cameraFov
    {
        get { return Mathf.Clamp(PlayerPrefs.GetFloat("CameraFov", 60.0f), 60.0f, 90.0f); }
        set { PlayerPrefs.SetFloat("CameraFov", value); }
    }

    public static bool windowedFullscreenMode
    {
        get { return PlayerPrefs.GetInt("WindowedFullscreenMode", 1) != 0; }
        set
        {
            PlayerPrefs.SetInt("WindowedFullscreenMode", value ? 1 : 0);
            if (Screen.fullScreen)
            {
                var current = Screen.fullScreenMode;
                if (current == FullScreenMode.FullScreenWindow)
                {
                    if (!value)
                        Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                }
                else if (current == FullScreenMode.ExclusiveFullScreen)
                {
                    if (value)
                        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                }
            }
        }
    }

    public static string playerName
    {
        get { 
            string name = PlayerPrefs.GetString("Name", "");
            if (name.Length > 13)
                return name.Substring(0, 13);
            return name;
        }
        set { PlayerPrefs.SetString("Name", value); }
    }

    public static float mouseSensitivity
    {
        get { return Mathf.Max(0.01f, PlayerPrefs.GetFloat("MouseSensitivity", 1.0f)); }
        set { PlayerPrefs.SetFloat("MouseSensitivity", value); }
    }


    public static float zoomSensitivity
    {
        get { return Mathf.Max(0.01f, PlayerPrefs.GetFloat("ZoomedSensitivity", 0.5f)); }
        set { PlayerPrefs.SetFloat("ZoomedSensitivity", value); }
    }
    
    public static bool autoEquipOnPickup
    {
        get { return PlayerPrefs.GetInt("AutoEquipOnPickup", 1) != 0; }
        set { PlayerPrefs.SetInt("AutoEquipOnPickup", value ? 1 : 0); }
    }
    
}