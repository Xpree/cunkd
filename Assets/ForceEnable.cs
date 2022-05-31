using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceEnable : MonoBehaviour
{
    public GameObject[] List;
    // Start is called before the first frame update
    void Start()
    {
        foreach (var go in List)
            go.SetActive(true);
    }

}
