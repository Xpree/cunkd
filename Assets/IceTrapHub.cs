using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class IceTrapHub : NetworkBehaviour
{
    [SerializeField] GameObject iceGadgetTrap;
    [SerializeField] GameSettings _settings;
    [HideInInspector]public GameObject trap;
    float throwForce;

    [SyncVar(hook = nameof(OnPosition))] public Vector3 positionSync;
    void Awake()
    {
        throwForce = _settings.IceGadget.ThrowForce;
    }

    [Command]
    public void sync(Vector3 pos)
    {
        positionSync = pos;
    }

    [Client]
    void OnPosition(Vector3 previous, Vector3 current)
    {
        trap.transform.position = current;
        trap.GetComponent<Rigidbody>().isKinematic = true;
        trap.GetComponent<IceGadgetTrap>().iceMachine.Trigger();
    }

    [Command]
    public void CmdSpawnIceGadget(Vector3 pos, Vector3 force)
    {
        RpcSpawnIceGadgetClient(pos, force);
    }

    [ClientRpc(includeOwner = false)]
    void RpcSpawnIceGadgetClient(Vector3 pos, Vector3 force)
    {
        InstantiateIceTrap(pos,force);
    }

    public void InstantiateIceTrap(Vector3 pos, Vector3 force, bool owner = false)
    {
        trap = Instantiate(iceGadgetTrap, pos, Quaternion.identity);
        trap.GetComponent<Rigidbody>().AddForce(force);
        trap.GetComponent<Rigidbody>().AddTorque(new Vector3(0, 100000, 0), ForceMode.Force);
        trap.GetComponent<IceGadgetTrap>().owner = owner;
        trap.GetComponent<IceGadgetTrap>().hub = this;
    }
}
