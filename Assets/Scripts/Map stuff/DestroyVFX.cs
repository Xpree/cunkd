using UnityEngine;

public class DestroyVFX : MonoBehaviour
{
    [Header("Destroys the particle system when it stops playing\n")]
    [SerializeField] ParticleSystem vfx;
    //Start is called before the first frame update
    private void FixedUpdate()
    {
        if (vfx.isStopped)
            Destroy(gameObject);
    }
}
