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

    [ClientRpc]
    void RpcTriggerExcludeOwner(string trigger, NetworkIdentity target)
    {
        if(target.hasAuthority == false)
            EventBus.Trigger(trigger, target.gameObject);
    }

    [Server]
    public static void TriggerExcludeOwner(string trigger, NetworkIdentity target)
    {
        if (target.isClient == false) // will be handled by Rpc call otherwise
        {
            if(target.connectionToClient != null)
                EventBus.Trigger(trigger, target.gameObject);
        }
        instance.RpcTriggerExcludeOwner(trigger, target);
    }

    [ClientRpc]
    void RpcTriggerAll(string trigger, NetworkIdentity target)
    {
        EventBus.Trigger(trigger, target.gameObject);
    }

    [Server]
    public static void TriggerAll(string trigger, NetworkIdentity target)
    {
        if (target.isClient == false) // will be handled by Rpc call otherwise
            EventBus.Trigger(trigger, target.gameObject);
        instance.RpcTriggerAll(trigger, target);
    }
}
