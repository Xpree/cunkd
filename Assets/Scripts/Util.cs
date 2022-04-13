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
}