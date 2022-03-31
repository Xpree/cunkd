using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

/// <summary>
/// Handle player input by responding to Fusion input polling, filling an input struct and then working with
/// that input struct in the Fusion Simulation loop.
/// </summary>
public class InputController : NetworkBehaviour, INetworkRunnerCallbacks
{
    private StarterAssets.StarterAssetsInputs _inputAsset;
    private PlayerController _player;
    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;
    private float _rotationVelocity = 0;

    /// <summary>
    /// Hook up to the Fusion callbacks so we can handle the input polling
    /// </summary>
    public override void Spawned()
    {
        _inputAsset = GetComponent<StarterAssets.StarterAssetsInputs>();
        _player = GetComponent<PlayerController>();
        // Technically, it does not really matter which InputController fills the input structure, since the actual data will only be sent to the one that does have authority,
        // but in the name of clarity, let's make sure we give input control to the gameobject that also has Input authority.
        if (Object.HasInputAuthority)
        {
            Runner.AddCallbacks(this);
        }

        Debug.Log("Spawned [" + this + "] IsClient=" + Runner.IsClient + " IsServer=" + Runner.IsServer + " HasInputAuth=" + Object.HasInputAuthority + " HasStateAuth=" + Object.HasStateAuthority);
    }

    /// <summary>
    /// Get Unity input and store them in a struct for Fusion
    /// </summary>
    /// <param name="runner">The current NetworkRunner</param>
    /// <param name="input">The target input handler that we'll pass our data to</param>
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        NetworkInputData inputData = new NetworkInputData();
        if (_player != null && _player.Object != null)
        {
            inputData.move = _inputAsset.move;
        }
        input.Set(inputData);
    }

    //private Vector3 CalculateAim()
    //{
    //	Vector3 mousePos = Input.mousePosition;

    //	RaycastHit hit;
    //	Ray ray = Camera.main.ScreenPointToRay(mousePos);

    //	Vector3 mouseCollisionPoint = Vector3.zero;
    //	// Raycast towards the mouse collider box in the world
    //	if (Physics.Raycast(ray, out hit, Mathf.Infinity, _mouseRayMask))
    //	{
    //		if (hit.collider != null)
    //		{
    //			mouseCollisionPoint = hit.point;
    //		}
    //	}

    //	Vector3 aimDirection = mouseCollisionPoint - _player.turretPosition;
    //	aimDirection.y = 0;
    //	aimDirection.Normalize();
    //	return aimDirection;
    //}

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    /// <summary>
    /// FixedUpdateNetwork is the main Fusion simulation callback - this is where
    /// we modify network state.
    /// </summary>
    public override void FixedUpdateNetwork()
    {

        // Get our input struct and act accordingly. This method will only return data if we
        // have Input or State Authority - meaning on the controlling player or the server.
        if (GetInput(out NetworkInputData input))
        {
            if (input.move.magnitude < 0.001)
            {
                _player.SetDirections(Vector3.zero);
            }
            else
            {

                var direction = new Vector3(input.move.x, 0.0f, input.move.y).normalized;

                // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
                // if there is a move input rotate player when the player is moving
                float targetRotation = 0.0f;
                if (input.move != Vector2.zero)
                {
                    targetRotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
                    float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref _rotationVelocity, RotationSmoothTime);

                    // rotate to face input direction relative to camera position
                    transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                }

                var targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;
                _player.SetDirections(targetDirection.normalized * Runner.DeltaTime);
            }
        }
        else
        {
            _player.SetDirections(Vector3.zero);
        }
        _player.Move();
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
}

/// <summary>
/// Our custom definition of an INetworkStruct. Keep in mind that
/// * bool does not work (C# does not define a consistent size on different platforms)
/// * Must be a top-level struct (cannot be a nested class)
/// * Stick to primitive types and structs
/// * Size is not an issue since only modified data is serialized, but things that change often should be compact (e.g. button states)
/// </summary>
public struct NetworkInputData : INetworkInput
{

    public Vector2 move;

}
