using Mirror;

// Network synchronized cooldown and charges with local prediction
// ment to be used by both client and server.
public class NetworkCooldown : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnCooldownChanged))] NetworkTimer cooldownTimer;
    NetworkTimer localTimer;

    [SyncVar(hook = nameof(OnChargesChanged))] int charges = -1;
    int localCharges = -1;

    public bool HasInfiniteCharges => localCharges < 0;
    public int Charges => System.Math.Max(localCharges, 0);

    

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

    public void SetCooldown(double duration)
    {
        localTimer = NetworkTimer.FromNow(duration);
        if (this.isServer)
            cooldownTimer = localTimer;
    }

    public void SetCharges(int count)
    {
        localCharges = count;
        if(this.isServer)
        {
            charges = count;
        }
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

    public bool HasCooldown => localTimer.HasTicked == false;

    // Sets cooldown and uses a charge if any are set
    // Returns false if on cooldown or no charges are left
    public bool Use(double cooldown)
    {
        if (localCharges == 0 || HasCooldown)
            return false;
        SetCooldown(cooldown);
        if (HasInfiniteCharges == false)
        {
            SetCharges(localCharges - 1);
        }
        return true;  
    }

    public bool UseCharge()
    {
        if (localCharges == 0)
            return false;
        if (HasInfiniteCharges == false)
        {
            SetCharges(localCharges - 1);
        }
        return true;
    }
}
