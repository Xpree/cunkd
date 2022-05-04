using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpectatorUI : MonoBehaviour
{
    [SerializeField] GameObject _spectatorUI;
    [SerializeField] TextMeshProUGUI _spectatorText;
    public void EnableSpectatorUI()
    {
        _spectatorUI.SetActive(true);
    }

    public void DisableSpectatorUI()
    {
        _spectatorUI.SetActive(false);
    }


    public void SetSpectating(GameObject player)
    {
        if (player)
            _spectatorText.text = player.GetComponent<GameClient>()?.PlayerName ?? string.Empty;
        else
            _spectatorText.text = string.Empty;
    }    
}
