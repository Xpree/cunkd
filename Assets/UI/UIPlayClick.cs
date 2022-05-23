using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnityEngine.UI.Button))]
public class UIPlayClick : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<UnityEngine.UI.Button>().onClick.AddListener(PlaySound);
    }

    void PlaySound()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/SoundStudents/SFX/Environment/MenuClick");
    }
}
