using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInputs : MonoBehaviour
{
	[HideInInspector]
	public InputAction Move;
	[HideInInspector]
	public InputAction Look;
	[HideInInspector]
	public InputAction Jump;

	[HideInInspector]
	public InputAction NextItem;
	[HideInInspector]
	public InputAction Interact;

	[HideInInspector]
	public InputAction PrimaryAttack;
	[HideInInspector]
	public InputAction SecondaryAttack;

	[HideInInspector]
	public InputAction SelectItem1;
	[HideInInspector]
	public InputAction SelectItem2;
	[HideInInspector]
	public InputAction SelectItem3;

	[HideInInspector]
	public InputAction SpectateNext;

	[HideInInspector]
	public InputAction ShowScoreboard;
	[HideInInspector]
	public InputAction ToggleMenu;

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

		ShowScoreboard = CommonActionMap.FindAction("Show Scoreboard");
		ToggleMenu = CommonActionMap.FindAction("Toggle Menu");

		SpectatorActionMap = Input.actions.FindActionMap("Spectator");
		SpectateNext = SpectatorActionMap.FindAction("Spectate Next");

		PlayerMovementActionMap = Input.actions.FindActionMap("Player Movement");
		Move = PlayerMovementActionMap.FindAction("Move");
		Look = PlayerMovementActionMap.FindAction("Look");
		Jump = PlayerMovementActionMap.FindAction("Jump");

		PlayerActionsActionMap = Input.actions.FindActionMap("Player Actions");
		NextItem = PlayerActionsActionMap.FindAction("Next Item");
		Interact = PlayerActionsActionMap.FindAction("Interact");
		PrimaryAttack = PlayerActionsActionMap.FindAction("Primary Attack");
		SecondaryAttack = PlayerActionsActionMap.FindAction("Secondary Attack");
		SelectItem1 = PlayerActionsActionMap.FindAction("Select Item 1");
		SelectItem2 = PlayerActionsActionMap.FindAction("Select Item 2");
		SelectItem3 = PlayerActionsActionMap.FindAction("Select Item 3");

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


    private void Update()
    {		
		if(ToggleMenu.triggered)
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

}
