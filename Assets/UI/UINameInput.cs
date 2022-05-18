using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UINameInput : MonoBehaviour
{
    [SerializeField] TMPro.TMP_InputField inputField;


    public void Check()
    {
        this.gameObject.SetActive(string.IsNullOrEmpty(Settings.playerName));
    }

    private void OnEnable()
    {
        inputField.text = Settings.playerName;
    }

    private void OnDisable()
    {
        Settings.playerName = inputField.text;
    }
}
