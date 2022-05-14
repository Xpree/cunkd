using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelEvents", menuName = "Scriptable Objects/Level Event")]
public class LevelEvents : ScriptableObject
{
    [Serializable]
    public class VolcanoLevelEvent
    {
        public float startTime;
        [SerializeField] public float runTime;
        [SerializeField] public Vector3 waterPosition;
        [SerializeField] public GameObject[] enableOnEvent;
        [SerializeField] public GameObject[] disableOnEvent;
        [SerializeField] public Rigidbody[] disableKinenmaticOnEvent;
        [HideInInspector] public bool triggered = false;

    }
    [SerializeField]
    public VolcanoLevelEvent[] VolcanoLevelEvents;
}
