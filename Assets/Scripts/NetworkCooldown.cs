using Mirror;

// Network synchronized cooldown and charges with local prediction
// ment to be used by both client and server.
public class NetworkCooldown : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnCooldownChanged))] NetworkTimer cooldownTimer;
    NetworkTimer localTimer;

    [SyncVar(hook = nameof(OnChargesChanged))] int charges = -1;
    int localCharges = -1;

    public bool HasInfiniteCharges => charges < 0;
    public int Charges => System.Math.Max(localCharges, 0);
    public bool HasCooldown => localTimer.Elapsed < 0;

    [Server]
    public void SetCharges(int count)
    {
        charges = count;
    }


    void OnCooldownChanged(NetworkTimer previous, NetworkTimer current)
    {
        if(localTimer.TickTime < current.TickTime)
        {
            localTimer = current;
        }
    }

    void OnChargesChanged(int previous, int current)
    {
        localCharges = current;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        cooldownTimer = NetworkTimer.Now;
    }

    [ClientRpc]
    void RpcForceUpdateClients(NetworkTimer cooldownTimer, int charges)
    {
        localTimer = cooldownTimer;
        localCharges = charges;
    }

    // Call on server if the client predicted wrong, i.e. the server says it's on cooldown but the client think it's not.
    [Server]
    public void ForceUpdateClients()
    {
        RpcForceUpdateClients(cooldownTimer, charges);
    }

    // Sets cooldown and uses a charge if any are set
    // Returns false if on cooldown or no charges are left
    [Server]
    public bool ServerUse(double cooldown)
    {
        if (cooldownTimer.Elapsed < 0 || ServerUseCharge() == false)
            return false;
        cooldownTimer = NetworkTimer.FromNow(cooldown);
        return true;
    }



    [Server]
    public bool ServerUseCharge()
    {
        if (charges == 0)
            return false;
        if (HasInfiniteCharges == false)
        {
            charges = charges - 1;
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
