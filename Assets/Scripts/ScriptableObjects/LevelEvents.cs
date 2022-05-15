using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelEvents", menuName = "Scriptable Objects/Level Event")]
public class LevelEvents : ScriptableObject
{
    [Serializable]
    public class VolcanoLevelEvent
    {
        [Header("Event times:")]
        [SerializeField] public float startTime;
        [SerializeField] public float runTime;
        [Header("Water Rising:")]
        [SerializeField] public Vector3 waterPosition;
        [HideInInspector] public bool triggered = false;
        [Header("Volcano Eruption:")]
        [SerializeField] public float eruptionDuration;
        [SerializeField] public float rockSpawnInterval;
        [SerializeField] public int maxSpawns;
        [SerializeField] public GameObject[] objectsToSpawn;

    }
    [SerializeField]
    public VolcanoLevelEvent[] VolcanoLevelEvents;
}
