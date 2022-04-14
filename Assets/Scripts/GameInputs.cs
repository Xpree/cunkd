using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInputs : MonoBehaviour
{
	[Header("Character Input Values")]
	public Vector2 Move;
	public bool MovePressed;
	public Vector2 Look;
	public bool Jump;

	public void OnMove(InputValue value)
	{
		MoveInput(value.Get<Vector2>());
	}

	public void OnLook(InputValue value)
	{
		LookInput(value.Get<Vector2>());
	}

	public void OnJump(InputValue value)
	{
		JumpInput(value.isPressed);
	}


	public void MoveInput(Vector2 newMoveDirection)
	{
		Move = newMoveDirection;
		MovePressed = newMoveDirection.sqrMagnitude > 0;
	}

	public void LookInput(Vector2 newLookDirection)
	{
		Look = newLookDirection;
	}

	public void JumpInput(bool newJumpState)
	{
		Jump = newJumpState;
	}


    private void FixedUpdate()
    {
		Jump = false;
    }
}
