using UnityEngine;
using Mirror;
using Unity.VisualScripting;

public class InteractionExample : MonoBehaviour
{
    public Collider interactCollider;

    private void OnEnable()
    {
        if (interactCollider == null)
        {
            Debug.LogError("Missing interact collider on " + this.name);
            return;
        }

        EventBus.Register(new EventHook(nameof(EventPlayerInteract), interactCollider.gameObject), new System.Action<NetworkIdentity>(OnInteract));
        EventBus.Register(new EventHook(nameof(EventPlayerInteractHoverStart), interactCollider.gameObject), new System.Action<NetworkIdentity>(OnInteractHoverStart));
        EventBus.Register(new EventHook(nameof(EventPlayerInteractHoverStop), interactCollider.gameObject), new System.Action<NetworkIdentity>(OnInteractHoverStop));
    }

    private void OnDisable()
    {
        EventBus.Unregister(new EventHook(nameof(EventPlayerInteract), interactCollider.gameObject), new System.Action<NetworkIdentity>(OnInteract));
        EventBus.Unregister(new EventHook(nameof(EventPlayerInteractHoverStart), interactCollider.gameObject), new System.Action<NetworkIdentity>(OnInteractHoverStart));
        EventBus.Unregister(new EventHook(nameof(EventPlayerInteractHoverStop), interactCollider.gameObject), new System.Action<NetworkIdentity>(OnInteractHoverStop));
    }

    void OnInteractHoverStart(NetworkIdentity player)
    {
        player.GetComponentInChildren<PlayerGUI>(true)?.SetInteraction("Interact with this item");
    }

    void OnInteractHoverStop(NetworkIdentity player)
    {
        player.GetComponentInChildren<PlayerGUI>(true)?.HideInteraction();
    }

    void OnInteract(NetworkIdentity player)
    {
        Debug.Log("You interacted with this item.");
    }

}
