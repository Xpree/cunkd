using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityGunAttractSound : MonoBehaviour
{
    FMOD.Studio.EventInstance attractSoundInstance;

    public void PlayOneShotAttachedWithParameters(string fmodEvent, GameObject gameObject, params (string name, float value)[] parameters)
    {
        FMOD.Studio.EventInstance instance = FMODUnity.RuntimeManager.CreateInstance(fmodEvent);

        foreach (var (name, value) in parameters)
        {
            instance.setParameterByName(name, value);
        }

        FMODUnity.RuntimeManager.AttachInstanceToGameObject(instance, gameObject.transform, gameObject.GetComponent<Rigidbody>());
        instance.start();        

        attractSoundInstance = instance;
    }

    public void AttractSound()
    {
        //AudioHelper.PlayOneShotAttachedWithParameters("event:/SoundStudents/SFX/Weapons/Gravity Gun", this.AnchorPoint.gameObject, ("Grab Object", 1f), ("Object recived start loading", 1f), ("Shot away object", 0f));
        PlayOneShotAttachedWithParameters("event:/SoundStudents/SFX/Weapons/Gravity Gun", this.gameObject, ("Grab Object", 1f), ("Object recived start loading", 1f), ("Shot away object", 0f));
    }

    public void StopSound()
    {
        attractSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        attractSoundInstance.release();        
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
