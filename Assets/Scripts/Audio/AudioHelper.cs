using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AudioHelper
{
    public static void PlayOneShotWithParameters(string fmodEvent, Vector3 position, params (string name, float value)[] parameters)
    {
        FMOD.Studio.EventInstance instance = FMODUnity.RuntimeManager.CreateInstance(fmodEvent);

        foreach (var (name, value) in parameters)
        {
            instance.setParameterByName(name, value);
        }
       
        instance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(position));
        instance.start();
        instance.release();
    }

    public static void PlayOneShotAttachedWithParameters(string fmodEvent, GameObject gameObject, params (string name, float value)[] parameters)
    {
        FMOD.Studio.EventInstance instance = FMODUnity.RuntimeManager.CreateInstance(fmodEvent);

        foreach (var (name, value) in parameters)
        {
            instance.setParameterByName(name, value);
        }            
        
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(instance, gameObject.transform, gameObject.GetComponent<Rigidbody>());
        instance.start();
        instance.release();
    }


    public static void PlayOneShotAttachedWithParameters(string fmodEvent, GameObject gameObject, float minRange, float maxRange, params (string name, float value)[] parameters)
    {
        FMOD.Studio.EventInstance instance = FMODUnity.RuntimeManager.CreateInstance(fmodEvent);

        instance.setProperty(FMOD.Studio.EVENT_PROPERTY.MINIMUM_DISTANCE, minRange);
        instance.setProperty(FMOD.Studio.EVENT_PROPERTY.MAXIMUM_DISTANCE, maxRange);

        foreach (var (name, value) in parameters)
        {
            instance.setParameterByName(name, value);
        }

        FMODUnity.RuntimeManager.AttachInstanceToGameObject(instance, gameObject.transform, gameObject.GetComponent<Rigidbody>());
        instance.start();
        instance.release();
    }
    /*public static void JetPackFlyingSound(GameObject gameObject)
    {
        //[SerializeField]
        FMOD.Studio.EventInstance jetpackSoundInstance = FMODUnity.RuntimeManager.CreateInstance("event:/SoundStudents/SFX/Gadgets/JetpackAcceleration");
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(jetpackSoundInstance, gameObject.transform, gameObject.GetComponent<Rigidbody>());
        jetpackSoundInstance.start();
    }*/
}
