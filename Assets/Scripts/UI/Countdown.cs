using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Countdown : MonoBehaviour
{
    [SerializeField] GameObject[] _objects;
    [SerializeField] AnimationCurve _countdownCurve;

    public void StartCountdown(NetworkTimer networkTime)
    {
        if (networkTime.HasTicked == false)
        {
            StartCoroutine(Countdown_Coroutine(networkTime));
        }
    }

    System.Collections.IEnumerator Countdown_Coroutine(NetworkTimer timer)
    {
        bool playedSound = false;

        ResetCounters();

        float t = (float)timer.Remaining;

        int lastCounter = _objects.Length;
        while (t > -1)
        {
            int counter = Mathf.CeilToInt(t);
            if(lastCounter != counter)
            {
                if (lastCounter < _objects.Length)
                {
                    _objects[lastCounter].SetActive(false);
                }
                _objects[counter].SetActive(true);
                lastCounter = counter;

                if(counter == 3)
                {
                    if (!playedSound)
                    {
                        playedSound = true;
                        FMODUnity.RuntimeManager.PlayOneShot("event:/SoundStudents/SFX/Environment/Announcer60BPM");
                    }
                }
            }
            
            if(counter < _objects.Length)
            {
                float x = t - Mathf.Floor(t);
                float scale = _countdownCurve.Evaluate(x);
                _objects[counter].transform.localScale = new Vector3(scale, scale, scale);
            }

            yield return null;
            t = (float)timer.Remaining;
        }

        ResetCounters();
    }

    private void ResetCounters()
    {
        for (int i = 0; i < _objects.Length; i++)
        {
            _objects[i].SetActive(false);
            _objects[i].transform.localScale = Vector3.one;
        }

    }

    public void StopCountdown()
    {
        ResetCounters();
        StopAllCoroutines();
    }
}
