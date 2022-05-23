using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepsSound : MonoBehaviour
{
    /*private void OnCollisionEnter(Collider other)
    {
        if (other.tag == "sand")
        {
            Debug.Log("sand sound");
        }
        if (other.tag == "wood")
        {
            Debug.Log("wood sound");
        }
        else
        {
            Debug.Log("concrete sound");
        }
    }*/       

    public static void Footsteps() 
    {
        Debug.Log("Footsteps sound");
        FMODUnity.RuntimeManager.PlayOneShot("event:/SoundStudents/SFX/Environment/Step sounds on dirt");
       
        /*GameObject gameObject = GameObject.Find("Plank (1)");
        Debug.Log(gameObject.name);

        GameObject gameObject2 = GameObject.Find("floating platform (2)");
        Debug.Log(gameObject2.name);

        string materialName = gameObject.GetComponent<Renderer>().material.name;
        Debug.Log(materialName);
        string materialName2 = gameObject2.GetComponent<Renderer>().material.name;
        Debug.Log(materialName2);
        if (materialName == "wood (Instance)")
        {
            Debug.Log("works for wood");
        }
        if (materialName2 == "BrownGrey (Instance)")
        {
            Debug.Log("works for sand");
        }*/
        
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
