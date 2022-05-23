using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetpackSound : MonoBehaviour
{
    FMOD.Studio.EventInstance jetpackSoundInstance;

    public void PlayOneShotAttachedWithParameters(string fmodEvent, GameObject gameObject, params (string name, float value)[] parameters)
    {
        FMOD.Studio.EventInstance instance = FMODUnity.RuntimeManager.CreateInstance(fmodEvent);

        foreach (var (name, value) in parameters)
        {
            instance.setParameterByName(name, value);
        }

        FMODUnity.RuntimeManager.AttachInstanceToGameObject(instance, gameObject.transform, gameObject.GetComponent<Rigidbody>());
        instance.start();

        jetpackSoundInstance = instance;
    }

    public void FlySound()
    {
        PlayOneShotAttachedWithParameters("event:/SoundStudents/SFX/Gadgets/Jetpack", this.gameObject, ("Gasar", 1f), ("Bränsle", 0f), ("Ta på jetpack", 0f), 
                                         ("Jetpack flyger tomgång", 0f), ("Jetpack stängs av", 0f), ("Sätt på jetpack", 0f));        
    }

    public void StopFlySound()
    {
        jetpackSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        jetpackSoundInstance.release();        
    }
}
