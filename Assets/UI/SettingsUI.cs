using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    public TMP_InputField playerNameInput;
    public Toggle autoEquipToggle;

    public Slider fieldOfView;
    public TMP_InputField mouseSensitivity;
    public TMP_InputField zoomMouseSensitivity;
    public Slider masterVolume;
    public Toggle muteToggle;

    bool invalidSettings = true;

    public GameObject hostUI;
    public GameObject connectedUI;

    public Button backButton;

    public GameObject fullscreenUI;
    public GameObject windowedUI;

    private void Awake()
    {
        playerNameInput.onValueChanged.AddListener(SetPlayerName);
        autoEquipToggle.onValueChanged.AddListener(SetAutoEquip);
        fieldOfView.onValueChanged.AddListener(SetFieldOfView);
        mouseSensitivity.onValueChanged.AddListener(SetMouseSensitivity);
        zoomMouseSensitivity.onValueChanged.AddListener(SetZoomSensitivity);
        masterVolume.onValueChanged.AddListener(SetMasterVolume);
        muteToggle.onValueChanged.AddListener(SetMuted);

        fullscreenUI.SetActive(Screen.fullScreen);
        windowedUI.SetActive(!Screen.fullScreen);
    }

    void UpdateSettings()
    {
        if (invalidSettings == false)
            return;

        playerNameInput.text = Settings.playerName;
        autoEquipToggle.isOn = Settings.autoEquipOnPickup;
        fieldOfView.value = Settings.cameraFov;
        mouseSensitivity.text = Settings.mouseSensitivity.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
        zoomMouseSensitivity.text = Settings.zoomSensitivity.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
        masterVolume.value = Settings.volume;
        muteToggle.isOn = Settings.muted;

        invalidSettings = false;
    }

    void OnEnable()
    {
        UpdateSettings();

        bool inGame = LobbyServer.Instance != null && !LobbyServer.Instance.IsLobbyActive;
        connectedUI.SetActive(inGame && Mirror.NetworkClient.active);
        bool isHost = Mirror.NetworkServer.active;
        hostUI.SetActive(inGame && isHost);
    }

    void OnDisable()
    {
        invalidSettings = true;
    }

    private void Start()
    {
        UpdateSettings();
    }

    public void SetPlayerName(string name)
    {
        if (invalidSettings) return;

        if (LobbyClient.Local != null && LobbyServer.Instance.IsLobbyActive == false)
        {
            return;
        }
        
        Settings.playerName = name;
        if (LobbyClient.Local != null && LobbyServer.Instance.IsLobbyActive)
        {
            LobbyClient.Local.CmdChangePlayerName(Settings.playerName);
        }
    }

    public void SetAutoEquip(bool value)
    {
        if (invalidSettings) return;

        Settings.autoEquipOnPickup = value;
    }

    public void SetFieldOfView(float value)
    {
        if (invalidSettings) return;

        Settings.cameraFov = value;
    }

    public void SetMouseSensitivity(string value)
    {
        float v;
        if (invalidSettings || float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out v) == false) return;

        Settings.mouseSensitivity = v;
    }

    public void SetZoomSensitivity(string value)
    {
        float v;
        if (invalidSettings || float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out v) == false) return;

        Settings.zoomSensitivity = v;
    }

    public void SetMasterVolume(float value)
    {
        if (invalidSettings) return;

        Settings.volume = value;
        AudioSettings.Singleton.MasterVolumeLevel(value);
    }

    public void SetMuted(bool value)
    {
        if (invalidSettings) return;

        Settings.muted = value;
        AudioSettings.Singleton.SetMuted(value);
    }

    public void Disconnect()
    {
        backButton.onClick.Invoke();
        CunkdNetManager.Disconnect();
    }

    public void EndRound()
    {
        backButton.onClick.Invoke();
        GameServer.Stats.ShowEndedByHost();
        GameServer.EndGame();
    }

    public void ToggleFullScreen()
    {
        if(Application.isEditor == false)
        {
            fullscreenUI.SetActive(!Screen.fullScreen);
            windowedUI.SetActive(Screen.fullScreen);
        }
        Util.ToggleFullscreen();
    }
}
