using UnityEngine;

public class nameLookAtCamera : MonoBehaviour
{

    private void LateUpdate()
    {
        if (Camera.main == null)
            return;
        var cameraTransform = Camera.main.transform;
        transform.LookAt(transform.position + cameraTransform.rotation * Vector3.forward, cameraTransform.rotation * Vector3.up);
    }

}
