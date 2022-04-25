using Mirror;

// Network synchronized cooldown and charges with local prediction
// ment to be used by both client and server.
public class NetworkCooldown : NetworkBehaviour
{
    NetworkTimer serverCooldownTimer;
    NetworkTimer localTimer;

    int serverCharges = -1;
    int localCharges = -1;

    public bool HasInfiniteCharges => serverCharges < 0;
    public int Charges => System.Math.Max(localCharges, 0);
    public bool HasCooldown => localTimer.Elapsed < 0;

    [Server]
    public void SetCharges(int count)
    {
        serverCharges = count;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        serverCooldownTimer = NetworkTimer.Now;
    }

    [ClientRpc]
    void RpcUpdateClients(NetworkTimer cooldownTimer, int charges)
    {
        localTimer = cooldownTimer;
        localCharges = charges;
    }

    // Sets cooldown and uses a charge if any are set
    // Returns false if on cooldown or no charges are left
    [Server]
    public bool ServerUse(double cooldown)
    {
        if (serverCooldownTimer.Elapsed < 0 || ServerUseCharge() == false)
            return false;
        serverCooldownTimer = NetworkTimer.FromNow(cooldown);
        RpcUpdateClients(serverCooldownTimer, serverCharges);
        return true;
    }

    [Server]
    public bool ServerUseCharge()
    {
        if (serverCharges == 0)
            return false;
        if (HasInfiniteCharges == false)
        {
            serverCharges = serverCharges - 1;
            RpcUpdateClients(serverCooldownTimer, serverCharges);
        }
        return true;
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
