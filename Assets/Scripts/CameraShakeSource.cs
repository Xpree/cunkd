using UnityEngine;


// Work in progress
public class CameraShakeSource : MonoBehaviour
{
    public CameraShake cameraShake;
    public float ActivationRange = 2.0f;

    [HideInInspector]
    public NetworkTimer ActivationTimer;

    public bool IsShaking => ActivationTimer.Elapsed < cameraShake.Duration;

    public void StartShake()
    {
        cameraShake.Initialize(NetworkTimer.Now);
    }

}
