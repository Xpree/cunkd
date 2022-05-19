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
    [Header("Each Index in the following array represents the corresponding event\n(player spawn points are updated 5 seconds before the event)")]
    [SerializeField] GameObject[] playerSpawnPositions;
    [Header("Each Index in the following array represents the corresponding event")]
    [SerializeField] GameObject[] setActiveAfterEvent;
    [Header("Each Index in the following array represents the corresponding event")]
    [SerializeField] GameObject[] setInactiveAfterEvent;
    [Header("Each Index in the following array represents the corresponding event")]
    [SerializeField] GameObject[] setUseGravityAfterEvent;
    int eventIndex = 0;

    Vector3 lastWaterPos;
    Vector3 lastBoatPos;
    Quaternion lastBoatRot;
    float yDiff;
    [Server]
    public override void OnStartServer()
    {
        lastBoatPos = boat.transform.position;
        lastWaterPos = water.transform.position;
        lastBoatRot = boat.transform.rotation;
        yDiff = Mathf.Abs(deathFloor.transform.position.y - water.transform.position.y);

        foreach (var item in setActiveAfterEvent)
        {
            item.SetActive(false);
        }
    }

    bool spawnPointsSet = false;
    void setPlayerSpawn()
    {
        scoreKeeper.setPlayerSpawnPositions(playerSpawnPositions[eventIndex]);
        spawnPointsSet = true;
    }

    [Server]
    void triggerEvent(LevelEvents.VolcanoLevelEvent levelEvent)
    {
        print("triggering event: " + (eventIndex) + " at time: " + GameStats.RoundTimer.Elapsed);
        levelEvent.triggered = true;
        runningEvent = true;
        lerpVal = 0;

        volcanoEruptor.Erupt(levelEvent.eruptionDuration, levelEvent.objectsToSpawn, levelEvent.rockSpawnInterval, levelEvent.maxSpawns);

        eventIndex++;
        spawnPointsSet = false;
        gravitySet = false;
    }
    bool runningEvent = false;
    LevelEvents.VolcanoLevelEvent nextEvent = new();
    float lerpVal = 0;

    //[ClientRpc]
    //void moveBoat(Vector3 pos, Quaternion rot)
    //{
    //    boat.transform.position = pos;
    //    boat.transform.rotation = rot;
    //}
    bool gravitySet = false;
    void setUseGravity(int eventIndex)
    {
        foreach (var item in setUseGravityAfterEvent[eventIndex].GetComponentsInChildren<Rigidbody>())
        {
            if (item.useGravity)
            {
                item.isKinematic = false;
            }
        }
        gravitySet = true;
    }


    [Server]
    void Update()
    {
        if (eventIndex < events.VolcanoLevelEvents.Length)
        {
            nextEvent = events.VolcanoLevelEvents[eventIndex];

            if (nextEvent.startTime - 5 < GameStats.RoundTimer.Elapsed && !spawnPointsSet)
            {
                setPlayerSpawn();
            }

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


                boat.transform.position = Vector3.Lerp(lastBoatPos, currentEvent.boatPosition, lerpVal);
                boat.transform.rotation = Quaternion.Lerp(lastBoatRot, currentEvent.boatRotation, lerpVal);

                //moveBoat(boat.transform.position, boat.transform.rotation);

            }
            if (1 < lerpVal)
            {
               // print("eventIndex: " + (eventIndex - 1) + " setUseGravityAfterEvent.Length: " + setUseGravityAfterEvent.Length);
                if (eventIndex <= setActiveAfterEvent.Length)
                {
                    setActiveAfterEvent[eventIndex - 1].SetActive(true);
                }
                if (eventIndex <= setInactiveAfterEvent.Length)
                {
                    setInactiveAfterEvent[eventIndex - 1].SetActive(false);
                }
                if (!gravitySet && eventIndex <= setUseGravityAfterEvent.Length)
                {
                    setUseGravity(eventIndex-1);
                }
                lastWaterPos = currentEvent.waterPosition;
                lastBoatPos = currentEvent.boatPosition;
                lastBoatRot = currentEvent.boatRotation;
                runningEvent = false;
            }
        }
    }
}
