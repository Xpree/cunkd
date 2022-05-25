using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMusic : MonoBehaviour
{
    public LevelMusic[] levelMusic;  
    public static SceneMusic singleton;
    private float lowLife;



    private void Awake()
    {
        singleton = this;
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;        
    }

    private void Start()
    {
        foreach (var music in levelMusic)
        {
            music.Initialize();
        }
        SceneManager_sceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    public void UpdateLives(int lives)
    {
        if (lives >= 3)
        {
            lowLife = 0f;
        }
        else
        {
            lowLife = 1f;
        }

        foreach (var music in levelMusic)
        {
            music.UpdateLowLife(lowLife);
        }
        
    }

    private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode arg1)
    {        
        bool found = false;
        foreach (var music in levelMusic)
        {                       
            if (music.scene == scene.path)
            {
                found = true;
                music.Play();
            }
            else
            {
                music.Stop();
            }
        }

        if(!found)
        {
            levelMusic[0].Play();
        }
    }        
}

[System.Serializable]
public class LevelMusic 
{
    [Mirror.Scene]
    public string scene;
    public string music;

    public FMOD.Studio.EventInstance _music;
    public bool activeMusic;

    public void Initialize()
    {
        _music = FMODUnity.RuntimeManager.CreateInstance(music);
    }

    public void UpdateLowLife(float lowLife)
    {
        if(activeMusic)
        {
            if(Mirror.NetworkServer.active == false)
                _music.setParameterByName("LowLife", lowLife, false);
        }
    }

    public void Stop()
    {
        if(activeMusic)
        {
            activeMusic = false;
            _music.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }
    
    public void Play() 
    {        
        if(activeMusic == false)
        {
            activeMusic = true;
            _music.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _music.setParameterByName("LowLife", 0, false);
            _music.start();
        }
    }

    

}
