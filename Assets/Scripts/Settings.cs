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
        get { return PlayerPrefs.GetString("Name", ""); }
        set { PlayerPrefs.SetString("Name", value); }
    }


    public static float mouseSensitivityYaw
    {
        get { return Mathf.Max(0.0001f, PlayerPrefs.GetFloat("MouseSensitivityYaw", 25.0f)); }
        set { PlayerPrefs.SetFloat("MouseSensitivityYaw", value); }
    }

    public static float mouseSensitivityPitch
    {
        get { return Mathf.Max(0.0001f, PlayerPrefs.GetFloat("MouseSensitivityPitch", 25.0f)); }
        set { PlayerPrefs.SetFloat("MouseSensitivityPitch", value); }
    }

    public static bool autoEquipOnPickup
    {
        get { return PlayerPrefs.GetInt("AutoEquipOnPickup", 1) != 0; }
        set { PlayerPrefs.SetInt("AutoEquipOnPickup", value ? 1 : 0); }
    }


}