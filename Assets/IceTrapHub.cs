using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class IceTrapHub : NetworkBehaviour
{
    [SerializeField] GameObject iceGadgetTrap;
    [SerializeField] float selfDestructTime;
    [SerializeField] GameSettings _settings;
    [HideInInspector]public GameObject trap;
    float throwForce;

    List<GameObject> traps = new();

    [SyncVar(hook = nameof(OnPosition))] public Vector3 positionSync;
    [SyncVar(hook = nameof(OnParent))] public Transform parentSync;
    void Awake()
    {
        throwForce = _settings.IceGadget.ThrowForce;
    }

    float endTime = float.MaxValue;
    public void ActivateSelfDestruction()
    {
        endTime = GameStats.RoundTimer + selfDestructTime;
    }

    void destroyAll()
    {
        foreach (var trap in traps)
        {
            if (trap)
            {
                if (0 < trap.GetComponent<SpreadMat>().iceMat.Count )
                    return;
                Destroy(trap);
            }
        }
        NetworkServer.Destroy(this.gameObject);
    }

    //[Server]
    private void Update()
    {
        if (endTime < GameStats.RoundTimer)
        {
            destroyAll();
        }
    }

    [Command]
    public void sync(Vector3 pos)
    {
        positionSync = pos;
    }

    [Command]
    public void setParent(Transform parent)
    {
        parentSync = parent;
    }

    [Client]
    void OnPosition(Vector3 previous, Vector3 current)
    {
        trap.transform.position = current;
        trap.GetComponent<Rigidbody>().isKinematic = true;
        trap.GetComponent<IceGadgetTrap>().iceMachine.parent = this.transform;
        trap.GetComponent<IceGadgetTrap>().iceMachine.Trigger();
    }

    [Client]
    void OnParent(Transform previous, Transform current)
    {
        this.transform.SetParent(current);
        trap.transform.SetParent(current);
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
        GameObject.Destroy(trap, GameServer.Instance.Settings.IceGadget.Duration * 2);        
        trap.GetComponent<Rigidbody>().AddForce(force);
        trap.GetComponent<Rigidbody>().AddTorque(new Vector3(0, 100000, 0), ForceMode.Force);
        trap.GetComponent<IceGadgetTrap>().owner = owner;
        trap.GetComponent<IceGadgetTrap>().hub = this;
        traps.Add(trap);
        AudioHelper.PlayOneShotAttachedWithParameters("event:/SoundStudents/SFX/Gadgets/Icy Floor Trap", trap, 30.0f, 40.0f, ("Shot", 1f), ("StandbyHum", 1f));
    }
}
