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

    public static void SetPhysicsSynchronized(NetworkIdentity identity, bool enable)
    {
        var netRigidbody = identity.GetComponent<Mirror.Experimental.NetworkRigidbody>();
        if (netRigidbody != null)
            netRigidbody.enabled = enable;
        var netTransform = identity.GetComponent<Mirror.NetworkTransform>();
        if (netTransform != null)
            netTransform.enabled = enable;
    }

    public static void SetClientPhysicsAuthority(NetworkIdentity identity, bool enable)
    {
        var netRigidbody = identity.GetComponent<Mirror.Experimental.NetworkRigidbody>();
        if (netRigidbody != null)
            netRigidbody.clientAuthority = enable;
        var netTransform = identity.GetComponent<Mirror.NetworkTransform>();
        if (netTransform != null)
            netTransform.clientAuthority = enable;
    }

    public static bool HasControl(this NetworkIdentity identity)
    {
        return identity.hasAuthority || (NetworkServer.active && identity.connectionToClient == null);
    }

    [Server]
    public static void Teleport(GameObject go, Vector3 position)
    {
        if (go == null)
        {
            return;
        }

        var player = go.GetComponent<PlayerMovement>();
        if (player != null)
        {            
            player.Teleport(position);
        }
        else
        {
            var networkTransform = go.GetComponent<NetworkTransform>();
            networkTransform.RpcTeleport(position);
            networkTransform.Reset();
            go.transform.position = position;            
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

    public static Vector3 SphereCastPointOrMaxDistance(Transform transform, float maxDistance, LayerMask targetMask, float radius)
    {
        RaycastHit hit;
        if (Physics.SphereCast(new Ray(transform.position, transform.forward), radius, out hit, maxDistance, targetMask, QueryTriggerInteraction.Ignore))
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

    public static void SetJumpForce(Rigidbody rb, float height)
    {
        float jumpForce = Mathf.Sqrt(Mathf.Abs((2.0f * rb.mass * Physics.gravity.y) * height));
        var vel = rb.velocity;
        if(vel.y < jumpForce)
        {
            vel.y = jumpForce;
            rb.velocity = vel;
        }
    }

    public static void ToggleFullscreen()
    {
        var res = Screen.currentResolution;
        if (Screen.fullScreen)
        {
            // Toggle back to 16 by 9 landscape at 75% height of the screen                
            var h = (res.height >> 2) * 3;
            var w = h * 16 / 9;
            Screen.SetResolution(w, h, FullScreenMode.Windowed, res.refreshRate);

        }
        else
        {
            Screen.SetResolution(res.width, res.height, Settings.windowedFullscreenMode ? FullScreenMode.FullScreenWindow : FullScreenMode.ExclusiveFullScreen, res.refreshRate);
        }
    }


    public static Ray ForwardRay(this Transform transform)
    {
        return new (transform.position, transform.forward);
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
    public double Remaining => TickTime - NetworkTime.time;
    public static NetworkTimer Now => new NetworkTimer { TickTime = NetworkTime.time };
    public static NetworkTimer FromNow(double duration) => new NetworkTimer { TickTime = NetworkTime.time + duration };

    public bool IsSet => TickTime > 0;
    public bool HasTicked => IsSet && Elapsed >= 0;
}
