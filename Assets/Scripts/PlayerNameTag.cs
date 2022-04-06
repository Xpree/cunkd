using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
public class PlayerNameTag : NetworkBehaviour
{
    public GameObject NameCanvas;
    //[SerializeField] public TextMeshProUGUI nameText;
    void Start()
    {
        if (isLocalPlayer)
        {
            NameCanvas.SetActive(false);
        }
    }
}
