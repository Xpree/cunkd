using Mirror;
using UnityEngine.Events;
using Unity.VisualScripting;
using UnityEngine;
using System;

// Network synchronized cooldown and charges with local prediction
// ment to be used by both client and server.
public class NetworkCooldown : NetworkBehaviour
{
    NetworkTimer serverCooldownTimer;
    NetworkTimer _localTimer;

    bool _cooldownStarted = false;
    void SetCooldown(bool value)
    {
        if (_cooldownStarted == value)
            return;
        _cooldownStarted = value;

        if(_cooldownStarted)
        {
            EventBus.Trigger(nameof(EventNetworkCooldownBegin), this.gameObject);
        }
        else
        {
            EventBus.Trigger(nameof(EventNetworkCooldownEnd), this.gameObject);
        }
    }

    public NetworkTimer localTimer 
    { 
        get { return _localTimer; }
        set
        {
            _localTimer = value;
            SetCooldown(_localTimer.Elapsed < 0);
        }
    }

    public double coolDownDuration;

    int serverCharges = -1;
    int localCharges = -1;

    public bool HasInfiniteCharges => localCharges < 0;
    public int Charges => System.Math.Max(localCharges, 0);
    public bool HasCooldown => localTimer.Elapsed < 0;

    private void FixedUpdate()
    {
        SetCooldown(HasCooldown);
    }

    [Server]
    public void SetCharges(int count)
    {
        serverCharges = count;
        localCharges = count;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        serverCooldownTimer = NetworkTimer.Now;
        localTimer = serverCooldownTimer;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        // Bad solution that will create unnecessary spam but will help late join
        CmdForceUpdateClients();
    }

    [Command(requiresAuthority = false)]
    void CmdForceUpdateClients()
    {
        RpcUpdateClients(serverCooldownTimer, serverCharges);
    }

    [ClientRpc]
    void RpcUpdateClients(NetworkTimer cooldownTimer, int charges)
    {
        localTimer = cooldownTimer;
        localCharges = charges;
    }

    bool DispenseServerCharge()
    {
        if (serverCharges == 0)
            return false;
        if (serverCharges > 0)
        {
            serverCharges = serverCharges - 1;
            localCharges = serverCharges;
        }
        return true;

    }


    // Sets cooldown and uses a charge if any are set
    // Returns false if on cooldown or no charges are left
    [Server]
    public bool ServerUse()
    {
        return ServerUse(coolDownDuration);
    }

    // Sets cooldown and uses a charge if any are set
    // Returns false if on cooldown or no charges are left
    [Server]
    public bool ServerUse(double cooldown)
    {
        if (serverCooldownTimer.Elapsed < 0 || DispenseServerCharge() == false)
            return false;
        serverCooldownTimer = NetworkTimer.FromNow(cooldown);
        localTimer = serverCooldownTimer;
        RpcUpdateClients(serverCooldownTimer, serverCharges);
        return true;
    }

    [Server]
    public bool ServerUseCharge()
    {
        if (DispenseServerCharge())
        {
            RpcUpdateClients(serverCooldownTimer, serverCharges);
            return true;
        }
        else
        {
            return false;
        }
    }

    [Client]
    public bool Use()
    {
        return this.Use(this.coolDownDuration);
    }

    // Sets cooldown and uses a charge if any are set
    // Returns false if on cooldown or no charges are left
    [Client]
    public bool Use(double cooldown)
    {
        if (HasCooldown || UseCharge() == false)
            return false;
        localTimer = NetworkTimer.FromNow(cooldown);
        return true;  
    }


    [Client]
    public bool UseCharge()
    {
        if (localCharges == 0)
            return false;
        if (HasInfiniteCharges == false)
        {
            localCharges = localCharges - 1;
        }
        return true;
    }
}
