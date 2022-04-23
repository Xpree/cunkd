using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public static class Util
{
    /// <summary>
    /// Returns true if the current running code can modify the objects transform/rigidbody
    /// </summary>
    public static bool HasPhysicsAuthority(GameObject go)
    {
        if (go == null)
            return false;

        var identity = go.GetComponent<NetworkIdentity>();

        if (identity == null)
        {
            // Not a network object
            return true;
        }

        return identity.HasControl();
    }

    public static bool HasControl(this NetworkIdentity identity)
    {
        return identity.hasAuthority || (NetworkServer.active && identity.connectionToClient == null);
    }

    public static void Teleport(GameObject go, Vector3 position)
    {
        go.transform.position = position;
        if (!Util.HasPhysicsAuthority(go))
        {
            var other = go.GetComponent<PlayerMovement>();
            if (other == null)
                return;
            other.Teleport(position);
        }
    }


    public static Transform GetOwnerAimTransform(NetworkItem item)
    {
        return GetPlayerInteractAimTransform(item.Owner);
    }

    public static Transform GetPlayerInteractAimTransform(GameObject go)
    {
        if (go != null)
        {
            return go.GetComponentInChildren<PlayerCameraController>()?.playerCamera?.transform;
        }
        return null;
    }

    public static Vector3 RaycastPointOrMaxDistance(Transform transform, float maxDistance, LayerMask targetMask)
    {
        RaycastHit hit;
        if (Physics.Raycast(new Ray(transform.position, transform.forward), out hit, maxDistance, targetMask, QueryTriggerInteraction.Ignore))
        {
            return hit.point;
        }
        else
        {
            return transform.position + transform.forward * maxDistance;
        }
    }

    public static bool RaycastPoint(Transform transform, float maxDistance, LayerMask targetMask, out Vector3 point)
    {
        if (Physics.Raycast(new Ray(transform.position, transform.forward), out RaycastHit hit, maxDistance, targetMask, QueryTriggerInteraction.Ignore))
        {
            point = hit.point;
            return true;
        }
        else
        {
            point = Vector3.zero;
            return false;
        }
    }
}

/// <summary>
/// Use with [SyncVar] or as parameters to remote procedure calls to synchronize a timed event across server and clients.
/// 
/// [SyncVar] NetworkTimer _timer;
/// 
/// public override void OnServerStart() {
///     _timer = new NetworkTimer.FromNow(10);
/// }
/// 
/// void FixedUpdate() {
///     if(_timer.HasTicked) {
///         Debug.Log("Timer has ticked");
///     }
/// }
/// </summary>
[Serializable]
public struct NetworkTimer
{
    public double TickTime;

    public double Elapsed => NetworkTime.time - TickTime;
    public static NetworkTimer Now => new NetworkTimer { TickTime = NetworkTime.time };
    public static NetworkTimer FromNow(double duration) => new NetworkTimer { TickTime = NetworkTime.time + duration };
    public bool HasTicked => TickTime > 0 && NetworkTime.time > TickTime;
}
