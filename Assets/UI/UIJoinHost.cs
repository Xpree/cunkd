using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIJoinHost : MonoBehaviour
{
    [SerializeField] TMPro.TMP_InputField inputField;
    [SerializeField] GameObject connectingUI;
    [SerializeField] GameObject inputUI;

    void OnEnable()
    {
        inputField.ActivateInputField();
        inputField.text = PlayerPrefs.GetString("UIJoinHost_Input", "");
        connectingUI.SetActive(false);
        inputUI.SetActive(true);
    }

    void OnDisable()
    {
        PlayerPrefs.SetString("UIJoinHost_Input", inputField.text);
        StopAllCoroutines();
    }

    System.Collections.IEnumerator TryConnect()
    {
        yield return new WaitForSeconds(0.5f);
        
        CunkdNetManager.Instance.StartClient();
        while (Mirror.NetworkClient.isConnecting)
        {
            yield return null;
        }

        if (Mirror.NetworkClient.isConnected == false)
        {
            connectingUI.SetActive(false);
            inputUI.SetActive(true);
        }
    }

    public void Connect()
    {
        CunkdNetManager.Instance.networkAddress = inputField.text;
        if (string.IsNullOrEmpty(CunkdNetManager.Instance.networkAddress))
            return;
        connectingUI.SetActive(true);
        inputUI.SetActive(false);
        StartCoroutine(TryConnect());
    }
}
