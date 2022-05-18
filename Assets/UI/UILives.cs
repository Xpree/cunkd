using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILives : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text LivesText;

    public void SetLives(int count)
    {
        LivesText.text = count.ToString();
    }
}
