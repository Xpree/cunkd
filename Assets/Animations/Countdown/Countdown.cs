using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Countdown : NetworkBehaviour
{
    Animator _animator;
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    [ClientRpc]
    public void RpcStartCountdown(double networkTime)
    {
        _animator.Play("Countdown");
    }
}
