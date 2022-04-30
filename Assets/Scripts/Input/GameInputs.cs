using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInputs : MonoBehaviour
{
	[HideInInspector]
	public InputActionMap PlayerMovementActionMap;
	[HideInInspector]
	public InputActionMap PlayerActionsActionMap;
	[HideInInspector]
	public InputActionMap SpectatorActionMap;
	[HideInInspector]
	public InputActionMap CommonActionMap;
	[HideInInspector]
	public PlayerInput Input;

	public enum InputMode
	{
		None = 0,
		Player,
		Spectator,
	}


	public InputMode Mode = InputMode.None;

	private void Awake()
    {
		Input = GetComponent<PlayerInput>();
		Input.enabled = false;

		CommonActionMap = Input.actions.FindActionMap("Common");
		CommonActionMap.Enable();


		SpectatorActionMap = Input.actions.FindActionMap("Spectator");

		PlayerMovementActionMap = Input.actions.FindActionMap("Player Movement");

		PlayerActionsActionMap = Input.actions.FindActionMap("Player Actions");
	}

    public void EnablePlayerMaps(bool enable)
    {
		if(enable)
        {
			PlayerActionsActionMap.Enable();
			PlayerMovementActionMap.Enable();
        }
		else
        {
			PlayerActionsActionMap.Disable();
			PlayerMovementActionMap.Disable();
		}
	}

	public void EnableSpectatorMaps(bool enable)
    {
		if(enable)
        {
			SpectatorActionMap.Enable();
        }
		else
        {
			SpectatorActionMap.Disable();
        }
    }

	void SetInputMode(InputMode mode)
    {
		switch(mode)
        {
			case InputMode.Player:
				EnableSpectatorMaps(false);
				EnablePlayerMaps(true);
				break;
			case InputMode.Spectator:
				EnableSpectatorMaps(true);
				EnablePlayerMaps(false);
				break;
			default:
				EnablePlayerMaps(false);
				EnableSpectatorMaps(false);
				break;

        }
    }


	public void SetSpectatorMode()
    {
		SetInputMode(InputMode.Spectator);
		this.Mode = InputMode.Spectator;
	}

	public void SetPlayerMode()
    {
		SetInputMode(InputMode.Player);
		this.Mode = InputMode.Player;
	}

	public void PreventInput()
    {
		SetInputMode(InputMode.None);
    }

	public void EnableInput()
    {
		SetInputMode(this.Mode);
    }

	public void ToggleMenu()
    {
		if (Cursor.lockState == CursorLockMode.Locked)
		{
			Cursor.lockState = CursorLockMode.None;
			PreventInput();
		}
		else
		{
			Cursor.lockState = CursorLockMode.Locked;
			EnableInput();
		}
	}
}
