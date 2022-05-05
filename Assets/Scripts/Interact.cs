using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Unity.VisualScripting;
using UnityEngine.InputSystem;

public class Interact : NetworkBehaviour
{
    public LayerMask interactLayerMask = ~0;
    public float interactMaxDistance = 2.0f;

    [HideInInspector]
    public Transform aimTransform;
    [HideInInspector]
    public GameObject interactAimObject = null;
    [HideInInspector]
    public bool hoveringOnAimObject = false;

    PlayerCameraController playerCameraController;

    GameInputs gameInputs;

    private void Start()
    {
        gameInputs = GetComponentInChildren<GameInputs>();
        aimTransform = Util.GetPlayerInteractAimTransform(this.gameObject);
        playerCameraController = GetComponentInChildren<PlayerCameraController>(true);
    }

    private void OnEnable()
    {
        EventBus.Register(new EventHook(nameof(EventPlayerCameraDeactivated), this), new System.Action<EmptyEventArgs>(OnPlayerCameraDeactivated));
    }

    private void OnDisable()
    {
        StopHovering();
        EventBus.Unregister(new EventHook(nameof(EventPlayerCameraDeactivated), this), new System.Action<EmptyEventArgs>(OnPlayerCameraDeactivated));
    }

    void OnPlayerCameraDeactivated(EmptyEventArgs args)
    {
        StopHovering();
    }

    public bool RaycastInteract(out RaycastHit hit)
    {
        return Physics.Raycast(aimTransform.position, aimTransform.forward, out hit, interactMaxDistance, interactLayerMask);
    }

    public bool TriggerInteract()
    {
        if (interactAimObject != null)
        {
            EventBus.Trigger(nameof(EventPlayerInteract), interactAimObject, this.netIdentity);
            return true;
        }
        else
        {
            return false;
        }
    }

    void StopHovering()
    {
        if(hoveringOnAimObject)
        {
            hoveringOnAimObject = false;
            if(interactAimObject != null && interactAimObject.activeSelf)
                EventBus.Trigger(nameof(EventPlayerInteractHoverStop), interactAimObject, this.netIdentity);
            interactAimObject = null;
        }
    }

    void SetHover(GameObject gameObject)
    {
        if (interactAimObject != gameObject && gameObject.activeSelf)
        {
            StopHovering();

            interactAimObject = gameObject;
            hoveringOnAimObject = true;
            EventBus.Trigger(nameof(EventPlayerInteractHoverStart), interactAimObject, this.netIdentity);
        }
    }
   

    private void FixedUpdate()
    {
        if(this.netIdentity.HasControl() && playerCameraController.IsCameraActive)
        {
            if (RaycastInteract(out RaycastHit hit))
            {
                var target = hit.collider.gameObject;
                SetHover(target);
            }
            else
            {
                StopHovering();
            }


        }
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            TriggerInteract();
        }
    }
    
}