using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Unity.VisualScripting;

public class NetworkEventBus : NetworkBehaviour
{
    public static NetworkEventBus instance;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this);
    }

    static void DoTrigger(string trigger, NetworkIdentity target)
    {
        if (target == null || target.isActiveAndEnabled == false)
        {
            return;
        }
        EventBus.Trigger(trigger, target.gameObject);
    }

    [ClientRpc]
    void RpcTriggerExcludeOwner(string trigger, NetworkIdentity target)
    {
        if(target == null || target.hasAuthority == false)
            DoTrigger(trigger, target);
    }

    [Server]
    public static void TriggerExcludeOwner(string trigger, NetworkIdentity target)
    {
        if (target.isServerOnly) // will be handled by Rpc call otherwise
        {
            if(target.connectionToClient != null)
                EventBus.Trigger(trigger, target.gameObject);
        }
        instance.RpcTriggerExcludeOwner(trigger, target);
    }

    [ClientRpc]
    void RpcTriggerAll(string trigger, NetworkIdentity target)
    {
        DoTrigger(trigger, target);
    }

    [Server]
    public static void TriggerAll(string trigger, NetworkIdentity target)
    {
        if (target.isServerOnly) // will be handled by Rpc call otherwise
            EventBus.Trigger(trigger, target.gameObject);
        instance.RpcTriggerAll(trigger, target);
    }
}
