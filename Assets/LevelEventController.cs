using UnityEngine;
using Mirror;

public class LevelEventController : NetworkBehaviour
{
    [SerializeField] ScoreKeeper scoreKeeper;
    [SerializeField] VolcanoErupter volcanoEruptor;
    [SerializeField] LevelEvents events;
    [SerializeField] GameObject water;
    [SerializeField] GameObject boat;
    [SerializeField] GameObject deathFloor;
    [Header("Each Index in the following array represents the corresponding event")]
    [SerializeField] GameObject[] playerSpawnPositions;
    [Header("Each Index in the following array represents the corresponding event")]
    [SerializeField] GameObject[] setActiveOnEvent;
    [Header("Each Index in the following array represents the corresponding event")]
    [SerializeField] GameObject[] setInactiveOnEvent;
    int eventIndex = 0;

    Vector3 lastWaterPos;
    float yDiff;
    [Server]
    public override void OnStartServer()
    {
        lastWaterPos = water.transform.position;
        yDiff = Mathf.Abs(deathFloor.transform.position.y - water.transform.position.y);
    }

    [Server]
    void triggerEvent(LevelEvents.VolcanoLevelEvent levelEvent)
    {
        print("triggering event: " + (eventIndex) + " at time: " + GameStats.RoundTimer.Elapsed);
        levelEvent.triggered = true;
        runningEvent = true;
        lerpVal = 0;

        scoreKeeper.setPlayerSpawnPositions(playerSpawnPositions[eventIndex]);
        volcanoEruptor.Erupt(levelEvent.eruptionDuration, levelEvent.objectsToSpawn, levelEvent.rockSpawnInterval, levelEvent.maxSpawns);

        eventIndex++;
    }
    bool runningEvent = false;
    LevelEvents.VolcanoLevelEvent nextEvent = new();
    float lerpVal = 0;

    [Server]
    void Update()
    {
        if (eventIndex < events.VolcanoLevelEvents.Length)
        {
            nextEvent = events.VolcanoLevelEvents[eventIndex];

            if (nextEvent.startTime < GameStats.RoundTimer.Elapsed && !runningEvent)
            {
                triggerEvent(events.VolcanoLevelEvents[eventIndex]);
            }
        }


        if (0 < eventIndex)
        {
            LevelEvents.VolcanoLevelEvent currentEvent = events.VolcanoLevelEvents[eventIndex - 1];
            if (currentEvent.startTime < GameStats.RoundTimer.Elapsed)
            {
                lerpVal = (float)(GameStats.RoundTimer.Elapsed - currentEvent.startTime) / currentEvent.runTime;
                water.transform.position = Vector3.Lerp(lastWaterPos, currentEvent.waterPosition, lerpVal);

                deathFloor.transform.position = new Vector3(deathFloor.transform.position.x, water.transform.position.y + yDiff, deathFloor.transform.position.z);


                boat.transform.position = Vector3.Lerp(boat.transform.position, currentEvent.boatPosition, lerpVal / 100);
                boat.transform.rotation = Quaternion.Lerp(boat.transform.rotation, currentEvent.boatRotation, lerpVal / 100);

            }
            if (1 < lerpVal)
            {
                if (eventIndex < setActiveOnEvent.Length)
                    setActiveOnEvent[eventIndex + 1].SetActive(true);
                if (eventIndex < setInactiveOnEvent.Length)
                    setInactiveOnEvent[eventIndex - 1].SetActive(false);
                lastWaterPos = currentEvent.waterPosition;
                runningEvent = false;
            }
        }
    }
}
