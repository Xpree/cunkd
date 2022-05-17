using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMusic : MonoBehaviour
{
    public LevelMusic[] levelMusic;

    //public LevelMusic[] levelMusic = { "event:/SoundStudents/Music/TitlePage&Menu", "event:/SoundStudents/Music/TitlePage&Menu", 
    //                                   "event:/SoundStudents/Music/Level 1 Theme", "event:/SoundStudents/Music/LavaLevel/LavaLevelAdaptive" };

    private void Awake()
    {
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        //Debug.Log("Hej");
    }

    private void Start()
    {
        levelMusic[0].Play();
    }

    private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode arg1)
    {
        Debug.Log(scene.name);
        foreach (var music in levelMusic)
        {
            if (music.scene == scene.name)
            {
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

    public void Play() 
    {
        
    }
}
