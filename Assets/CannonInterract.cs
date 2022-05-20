using UnityEngine;
using Mirror;
using Unity.VisualScripting;

public class CannonInterract : MonoBehaviour
{
    public Collider interactCollider;
    public GameObject AimDirection;
    public float Force;

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
        player.GetComponentInChildren<PlayerGUI>(true)?.SetInteraction("Launch self!");
    }

    void OnInteractHoverStop(NetworkIdentity player)
    {
        player.GetComponentInChildren<PlayerGUI>(true)?.HideInteraction();
    }

    void OnInteract(NetworkIdentity player)
    {
        Debug.Log("You interacted with this item.");
        Vector3 Aim = AimDirection.transform.position;
        AudioHelper.PlayOneShotWithParameters("event:/SoundStudents/SFX/Weapons/Gravity Gun", this.transform.position, ("Grab Object", 1f), ("Object recived start loading", 1f), ("Shot away object", 1f));
        player.GetComponent<Rigidbody>().AddForce(Aim * Force);
    }

}