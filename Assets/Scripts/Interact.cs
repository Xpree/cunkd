using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Unity.VisualScripting;

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

    private void Start()
    {
        aimTransform = Util.GetPlayerInteractAimTransform(this.gameObject);
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
            if(interactAimObject != null)
                EventBus.Trigger(nameof(EventPlayerInteractHoverEnd), interactAimObject, this.netIdentity);
            interactAimObject = null;
            FindObjectOfType<PlayerGUI>()?.interactiveButton(null);
        }
    }

    void SetHover(GameObject gameObject)
    {
        if (interactAimObject != gameObject)
        {
            StopHovering();

            interactAimObject = gameObject;
            hoveringOnAimObject = true;
            EventBus.Trigger(nameof(EventPlayerInteractHoverStart), interactAimObject, this.netIdentity);
            ObjectSpawner obs = gameObject.GetComponent<ObjectSpawner>();
            if (obs != null)
                FindObjectOfType<PlayerGUI>()?.interactiveButton(obs);
        }
    }

    private void FixedUpdate()
    {
        if(this.netIdentity.HasControl())
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

    // TODO: Remove this
    private void OnGUI()
    {
        if (!isLocalPlayer)
            return;        
        GUI.Box(new Rect(Screen.width * 0.5f - 1, Screen.height * 0.5f - 1, 2, 2), GUIContent.none);
    }
}