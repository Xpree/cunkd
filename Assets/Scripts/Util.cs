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

        if (NetworkClient.active)
        {
            var identity = go.GetComponent<NetworkIdentity>();
            if (identity == null)
            {
                // Not a network object
                return true;
            }

            // Returns true for every network object the client owns
            if (identity.hasAuthority)
                return true;
        }

        // Must not be 'else' because host is both server and client
        if (NetworkServer.active)
        {
            // Server is authoritive over everything except Players (objects with GameClient component)
            return go.GetComponent<GameClient>() == null;
        }
        return false;
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

    public static NetworkTimer FromNow(double duration) => new NetworkTimer { TickTime = NetworkTime.time + duration };
    public bool HasTicked => TickTime > 0 && NetworkTime.time > TickTime;
}
