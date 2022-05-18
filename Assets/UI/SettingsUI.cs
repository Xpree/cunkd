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
    public Slider mouseSensitivity;
    public Slider masterVolume;
    public Toggle muteToggle;

    bool invalidSettings = true;

    public GameObject hostUI;
    public GameObject connectedUI;

    public Button backButton;

    private void Awake()
    {
        playerNameInput.onValueChanged.AddListener(SetPlayerName);
        autoEquipToggle.onValueChanged.AddListener(SetAutoEquip);
        fieldOfView.onValueChanged.AddListener(SetFieldOfView);
        mouseSensitivity.onValueChanged.AddListener(SetMouseSensitivity);
        masterVolume.onValueChanged.AddListener(SetMasterVolume);
        muteToggle.onValueChanged.AddListener(SetMuted);
    }

    void UpdateSettings()
    {
        if (invalidSettings == false)
            return;

        playerNameInput.text = Settings.playerName;
        autoEquipToggle.isOn = Settings.autoEquipOnPickup;
        fieldOfView.value = Settings.cameraFov;
        mouseSensitivity.value = Settings.mouseSensitivityPitch;
        masterVolume.value = Settings.volume;
        muteToggle.isOn = Settings.muted;

        invalidSettings = false;
    }

    void OnEnable()
    {
        UpdateSettings();

        bool inGame = !LobbyServer.Instance.IsLobbyActive;
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

    public void SetMouseSensitivity(float value)
    {
        if (invalidSettings) return;

        Settings.mouseSensitivityPitch = value;
        Settings.mouseSensitivityYaw = value;
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
}
