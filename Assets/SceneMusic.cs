using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMusic : MonoBehaviour
{
    public LevelMusic[] levelMusic;       

    private void Awake()
    {
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;        
    }

    private void Start()
    {
        levelMusic[0].Play();
    }

    private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode arg1)
    {        
        foreach (var music in levelMusic)
        {                       
            if (music.scene == scene.path && scene.name != "LobbyScene")
            {                
                Debug.Log(music.scene);
                music.Play();
            }
        }
    }        
}

[System.Serializable]
public class LevelMusic 
{
    [Mirror.Scene]
    public string scene;
    public string music;

    public static FMOD.Studio.EventInstance _music;
    
    public void Play() 
    {        
        _music.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _music = FMODUnity.RuntimeManager.CreateInstance(music);
        _music.start();
        _music.release();                
    }
}
