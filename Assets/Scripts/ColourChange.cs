using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ColourChange : NetworkBehaviour
{
    [SerializeField] private Material newcolour;

    private void OnCollisionEnter(Collision WillGetPaintjob)
    {
        Color newpaintjob = RandomColourPicker();
        GetComponent<Renderer>().material.color = newpaintjob;
    }
    private Color RandomColourPicker()
    {
        return new Color(
            r: UnityEngine.Random.Range(0f, 1f),
            g: UnityEngine.Random.Range(0f, 1f),
            b: UnityEngine.Random.Range(0f, 1f));
    }
}
