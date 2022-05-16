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

        Settings.playerName = name;
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
}
