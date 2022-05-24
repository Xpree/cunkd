using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBlink : MonoBehaviour
{
    public float blinkSpeed = 3f;
    public TMPro.TMP_Text text;
    public Color blinkColor;

    Color originalColor;
    private void Awake()
    {
        originalColor = text.color;
    }

    private void OnEnable()
    {        
        StartCoroutine(Blink());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        text.enabled = true;
        text.color = originalColor;   
    }

    private IEnumerator Blink()
    {
        text.color = blinkColor;
        for(; ;)
        {
            text.enabled = !text.enabled;
            yield return new WaitForSeconds(1.0f / blinkSpeed);
        }
    }
}
