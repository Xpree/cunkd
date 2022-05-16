using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIErrorDialog : MonoBehaviour
{
    public static UIErrorDialog errorDialog;

    public static string errorText = null;

    public TextMeshProUGUI messageText;

    void Start()
    {
        errorDialog = this;
        if(errorText == null)
        {
            this.gameObject.SetActive(false);
        }
        else
        {
            messageText.text = errorText;
        }
    }

    private void OnDestroy()
    {
        if(errorDialog == this)
        {
            errorDialog = null;
        }
    }

    public void ClearError()
    {
        errorText = null;
    }

    public static void ShowError(string message)
    {
        if(errorDialog == null)
        {
            errorDialog = FindObjectOfType<UIErrorDialog>(true);
        }

        if(errorDialog != null)
        {
            errorDialog.gameObject.SetActive(true);
            errorDialog.messageText.text = message;
        }
        
        errorText = message;
    }
}
