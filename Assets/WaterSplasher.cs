using UnityEngine;
using Mirror;

public class WaterSplasher : MonoBehaviour
{
    [SerializeField] GameObject splash;
    [SerializeField] float heightAdjustment=0;

    [Client]
    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb)
        {
            //Debug.Log("splash!");
            FMODUnity.RuntimeManager.PlayOneShotAttached("event:/SoundStudents/SFX/Environment/Water splash sounds", rb.gameObject);
            GameObject pe = Instantiate(splash, other.transform.position + new Vector3(0, heightAdjustment, 0), splash.transform.rotation);
            pe.transform.localScale += Vector3.one * Mathf.Log(rb.mass) / 5;
        }
        else
        {
            rb = other.transform.parent.GetComponent<Rigidbody>();
            if (rb)
            {
                GameObject pe = Instantiate(splash, other.transform.position + new Vector3(0, heightAdjustment, 0), splash.transform.rotation);
                pe.transform.localScale += Vector3.one * Mathf.Log(rb.mass) / 5;
            }
        }
    }
}
