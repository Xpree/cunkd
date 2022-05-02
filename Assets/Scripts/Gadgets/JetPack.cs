using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.VFX;
using Unity.VisualScripting;

[RequireComponent(typeof(NetworkItem))]
[RequireComponent(typeof(NetworkCooldown))]
public class JetPack : NetworkBehaviour
{
    [SerializeField] bool isPassive;
    [SerializeField] int Charges;
    [SerializeField] float Cooldown = 0.05f;
    [SerializeField] float maxForce = 1.0f;
    [SerializeField] float acceleration = 0.01f;

    NetworkCooldown cooldownTimer;
    NetworkItem item;

    float force = 0;
    bool isUsing = false;

    private void Awake()
    {
        item = GetComponent<NetworkItem>();
        item.ItemType = ItemType.Gadget;

        cooldownTimer = GetComponent<NetworkCooldown>();
        cooldownTimer.CooldownDuration = Cooldown;
        cooldownTimer.MaxCharges = Charges;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        cooldownTimer.SetCharges(Charges);
    }

    [TargetRpc]
    void TargetTell(string message)
    {
        print(message);
    }

    [Server]
    void SetUsing(bool value)
    {
        if (isUsing == value)
        {
            return;
        }

        isUsing = value;

        // Reset force on when stopping/starting
        force = 0;

        if (isUsing)
        {
            NetworkEventBus.TriggerAll(nameof(EventPrimaryAttackBegin), this.netIdentity);
        }
        else
        {
            NetworkEventBus.TriggerAll(nameof(EventPrimaryAttackEnd), this.netIdentity);
        }
    }

    System.Collections.IEnumerator DestroyItem()
    {
        // Delay Destruction to allow for events to trigger
        yield return new WaitForSeconds(0.5f);
        NetworkServer.Destroy(this.gameObject);
    }

    [Server]
    void Fly()
    {
        if (isUsing && cooldownTimer.ServerUse(this.Cooldown))
        {
            force = Mathf.Min(force += acceleration, maxForce);
            RpcFlyLikeSatan(force);
            if (cooldownTimer.Charges == 0)
            {
                SetUsing(false);
                StartCoroutine(DestroyItem());
                return;
            }
        }
    }

    [ServerCallback]
    private void FixedUpdate()
    {
        SetUsing(item.AnyAttackPressed && cooldownTimer.Charges > 0);
        Fly();
    }

    [TargetRpc]
    void RpcFlyLikeSatan(float height)
    {
        item.GetOwnerComponent<PlayerMovement>()?.ApplyJumpForce(height);
    }
}
