using UnityEngine;
using Mirror;

public class LevelEventController : NetworkBehaviour
{
    [SerializeField] ScoreKeeper scoreKeeper;
    [SerializeField] LevelEvents events;
    [SerializeField] GameObject water;
    [SerializeField] Collider deathFloor;
    [SerializeField] GameObject[] playerSpawnPositions;
    [SerializeField] GameObject[] setActiveOnEvent;
    [SerializeField] GameObject[] setInactiveOnEvent;
    int eventIndex = 0;

    Vector3 lastWaterPos;
    public override void OnStartServer()
    {
        lastWaterPos = water.transform.position;
        //_endTime = NetworkTimer.FromNow(_settings.IceGadget.Duration);
    }

    void triggerEvent(LevelEvents.VolcanoLevelEvent levelEvent)
    {
        print("triggering event: " +(eventIndex) + " at time: " + NetworkTime.time);
        levelEvent.triggered = true;
        runningEvent = true;
        lerpVal = 0;

        scoreKeeper.setPlayerSpawnPositions(playerSpawnPositions[eventIndex]);

        eventIndex++;
    }
    bool runningEvent = false;
    LevelEvents.VolcanoLevelEvent nextEvent = new();
    float lerpVal = 0;
    //Update is called once per frame
    void Update()
    {
        if (eventIndex < events.VolcanoLevelEvents.Length)
        {
            nextEvent = events.VolcanoLevelEvents[eventIndex];

            if (nextEvent.startTime < NetworkTime.time && !runningEvent)
            {
                triggerEvent(events.VolcanoLevelEvents[eventIndex]);
            }
        }


        if (0 < eventIndex)
        {
            LevelEvents.VolcanoLevelEvent currentEvent = events.VolcanoLevelEvents[eventIndex - 1];
            if (currentEvent.startTime < NetworkTime.time)
            {
                lerpVal = (float)(NetworkTime.time - currentEvent.startTime) / currentEvent.runTime;
                water.transform.position = Vector3.Lerp(lastWaterPos, currentEvent.waterPosition, lerpVal);
                deathFloor.transform.position = water.transform.position - new Vector3(0, 10, 0);
            }
            if (1 < lerpVal)
            {
                if (eventIndex < setActiveOnEvent.Length)
                    setActiveOnEvent[eventIndex+1].SetActive(true);
                if (eventIndex < setInactiveOnEvent.Length)
                    setInactiveOnEvent[eventIndex-1].SetActive(false);
                lastWaterPos = currentEvent.waterPosition;
                runningEvent = false;
            }
        }
    }
}
