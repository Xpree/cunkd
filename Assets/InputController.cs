using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

/// <summary>
/// Handle player input by responding to Fusion input polling, filling an input struct and then working with
/// that input struct in the Fusion Simulation loop.
/// </summary>
public class InputController : NetworkBehaviour
{
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
        _player = GetComponent<PlayerController>();
        Debug.Log("Spawned [" + this + "] IsClient=" + Runner.IsClient + " IsServer=" + Runner.IsServer + " HasInputAuth=" + Object.HasInputAuthority + " HasStateAuth=" + Object.HasStateAuthority);
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
