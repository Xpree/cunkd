using Mirror;
using UnityEngine;
using Unity.VisualScripting;

// Network synchronized cooldown and charges with local prediction
// ment to be used by both client and server.
public class NetworkCooldown : NetworkBehaviour
{
    [HideInInspector]
    public float CooldownDuration;
    public float CooldownRemaining => Mathf.Max((float)_localTimer.Remaining, 0);

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

    NetworkTimer localTimer 
    { 
        get { return _localTimer; }
        set
        {
            _localTimer = value;
            SetCooldown(_localTimer.Elapsed < 0);
        }
    }
    public bool HasCooldown => localTimer.Elapsed < 0;

    int maxCharges = -1;

    int serverCharges = -1;
    int localCharges = -1;
    public int Charges => System.Math.Max(localCharges, 0);

    public bool HasInfiniteCharges => maxCharges < 0;
    public int MaxCharges
    {
        get { return System.Math.Max(maxCharges, 0); }
        set { maxCharges = value; }
    }

    private void FixedUpdate()
    {
        SetCooldown(HasCooldown);
    }

    [Server]
    public void SetCharges(int count)
    {
        serverCharges = count;
        localCharges = count;
        maxCharges = count;
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
        DoUpdateClients();
    }

    [ClientRpc]
    void RpcUpdateClients(NetworkTimer cooldownTimer, int charges, int maximumCharges)
    {
        localTimer = cooldownTimer;
        localCharges = charges;
        maxCharges = maximumCharges;
    }

    void DoUpdateClients()
    {
        RpcUpdateClients(serverCooldownTimer, serverCharges, maxCharges);
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
        return ServerUse(CooldownDuration);
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
        DoUpdateClients();
        return true;
    }

    [Server]
    public bool ServerUseCharge()
    {
        if (DispenseServerCharge())
        {
            DoUpdateClients();
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
        return this.Use(this.CooldownDuration);
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
