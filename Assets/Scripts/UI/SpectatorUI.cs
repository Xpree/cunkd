using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpectatorUI : MonoBehaviour
{
    [SerializeField] GameObject _spectatorUI;
    public void EnableSpectatorUI()
    {
        _spectatorUI.SetActive(true);
    }

    public void DisableSpectatorUI()
    {
        _spectatorUI.SetActive(false);
    }
}
