using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class GlobalInput : MonoBehaviour
{

    string senseYawInput;
    string sensePitchInput;
    private void Start()
    {
        sensePitchInput = Settings.mouseSensitivityPitch.ToString();
        senseYawInput = Settings.mouseSensitivityYaw.ToString();

        FMODUnity.RuntimeManager.MuteAllEvents(Settings.muted);
    }

    void Update()
    {
        if (Keyboard.current.altKey.isPressed && Keyboard.current.enterKey.wasPressedThisFrame)
        {
            Util.ToggleFullscreen();
        }
    }

    static bool TryGUIFloatField(ref string backingString, out float value)
    {
        var result = GUILayout.TextField(backingString);

        if(result != backingString)
        {
            if(float.TryParse(result, out value))
            {
                backingString = result;
                return true;
            }
        }
        value = 0f;
        return false;
    }

    void DoSettingsWindow(int windowID)
    {
        GUILayout.Label("Move: WASD\nAim: Mouse\nJump (and double jump): Space\nNext Item: Q\nSelect Item: 1,2,3\nInteract: E");

        GUILayout.BeginHorizontal();
        GUILayout.Label("Player Name:");
        Settings.playerName = GUILayout.TextField(Settings.playerName);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Auto Equip:");
        Settings.autoEquipOnPickup = GUILayout.Toggle(Settings.autoEquipOnPickup, GUIContent.none);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Horizontal mouse sensitivity:");
        if(TryGUIFloatField(ref senseYawInput, out float yaw))
        {
            Settings.mouseSensitivityYaw = yaw;
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Vertical mouse sensitivity:");
        if(TryGUIFloatField(ref sensePitchInput, out float pitch))
        {
            Settings.mouseSensitivityPitch = pitch;
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Volume:");
        Settings.volume = GUILayout.HorizontalSlider(Settings.volume, 0, 1);
        AudioSettings.Singleton.MasterVolumeLevel(Settings.volume);
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        GUILayout.Label("Camera field of view:");
        Settings.cameraFov = GUILayout.HorizontalSlider(Settings.cameraFov, 60, 90);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Mute:");
        var previouslyMuted = Settings.muted;
        Settings.muted = GUILayout.Toggle(Settings.muted, GUIContent.none);
        if(previouslyMuted != Settings.muted)
            AudioSettings.Singleton.SetMuted(Settings.muted);            
        GUILayout.EndHorizontal();

        if (!Application.isEditor)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Fullscreen (Alt+Enter):");
            var fullScreen = GUILayout.Toggle(Screen.fullScreen, GUIContent.none);
            GUILayout.EndHorizontal();

            if (Screen.fullScreen != fullScreen)
            {
                Util.ToggleFullscreen();
            }
        }

        if (NetworkClient.active || NetworkServer.active)
        {
            if (GUILayout.Button("Disconnect"))
            {
                CunkdNetManager.Disconnect();
            }
        }

        if (NetworkServer.active)
        {
            if (LobbyServer.Instance.IsLobbyActive == false && GUILayout.Button("End Game"))
                GameServer.EndGame();
        }
    }

    Rect settingsWindowRect = new Rect(20, 200, 300, 50);
    private void OnGUI()
    {
       if (Cursor.lockState == CursorLockMode.None)
       {
            settingsWindowRect.x = Screen.width * 0.5f - settingsWindowRect.width * 0.5f;
            settingsWindowRect.y = Screen.height * 0.5f - settingsWindowRect.height * 0.5f;

            settingsWindowRect = GUILayout.Window(0, settingsWindowRect, DoSettingsWindow, "Settings");
       }
    }
}
