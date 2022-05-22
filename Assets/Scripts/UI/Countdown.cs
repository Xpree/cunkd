using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Countdown : MonoBehaviour
{
    Animator _animator;
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void StartCountdown(NetworkTimer networkTime)
    {
        if (networkTime.HasTicked == false)
        {
            _animator.Play("Countdown");
            FMODUnity.RuntimeManager.PlayOneShot("event:/SoundStudents/SFX/Environment/Announcer60BPM");
        }
    }

    public void StopCountdown()
    {
        _animator.Play("Idle");
    }
}
