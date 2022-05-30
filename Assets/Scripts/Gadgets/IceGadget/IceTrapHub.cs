using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class IceTrapHub : NetworkBehaviour
{
    [SerializeField] GameObject iceGadgetTrap;
    [SerializeField] float selfDestructTime;
    [SerializeField] GameSettings _settings;
    float throwForce;

    List<GameObject> traps = new();

    [SyncVar(hook = nameof(OnIndex))] public int indexSync;
    [SyncVar(hook = nameof(OnPosition))] public Vector3 positionSync;
    [SyncVar] public Transform parentSync;
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

    private void Update()
    {
        if (endTime < GameStats.RoundTimer)
        {
            destroyAll();
        }
        syncTrap();
    }

    bool indexSet = false;
    bool positionSet = false;
    void syncTrap()
    {
        if (indexSet && positionSet)
        {

            if (!traps[indexSync-1].GetComponent<IceGadgetTrap>().owner)
            {
                traps[indexSync-1].transform.position = positionSync;
                traps[indexSync-1].GetComponent<Rigidbody>().isKinematic = true;
                traps[indexSync-1].GetComponent<IceGadgetTrap>().iceMachine.Trigger();
                if (parentSync)
                {
                    traps[indexSync-1].GetComponent<IceGadgetTrap>().transform.SetParent(parentSync, true);
                }
            }
            indexSet = false;
            positionSet = false;
        }
    }

    [Command]
    public void sync(int index, Vector3 pos, Transform parent)
    {
        parentSync = parent;
        indexSync = index;
        positionSync = pos;
    }

    [Client]
    void OnIndex(int previous, int current)
    {
        indexSet = true;
    }

    [Client]
    void OnPosition(Vector3 previous, Vector3 current)
    {
        positionSet = true;
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

    int currentTrapIndex = 1;
    public void InstantiateIceTrap(Vector3 pos, Vector3 force, bool owner = false)
    {
        GameObject trap = Instantiate(iceGadgetTrap, pos, Quaternion.identity);
        GameObject.Destroy(trap, GameServer.Instance.Settings.IceGadget.Duration * 2);        
        trap.GetComponent<Rigidbody>().AddForce(force);
        trap.GetComponent<Rigidbody>().AddTorque(new Vector3(0, 100000, 0), ForceMode.Force);
        trap.GetComponent<IceGadgetTrap>().owner = owner;
        trap.GetComponent<IceGadgetTrap>().hub = this;
        trap.GetComponent<IceGadgetTrap>().index = currentTrapIndex++;
        traps.Add(trap);
        AudioHelper.PlayOneShotAttachedWithParameters("event:/SoundStudents/SFX/Gadgets/Icy Floor Trap", trap, 30.0f, 40.0f, ("Shot", 1f), ("StandbyHum", 1f));
    }
}
