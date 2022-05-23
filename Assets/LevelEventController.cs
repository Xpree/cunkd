using UnityEngine;
using Mirror;
using TMPro;

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

    [SerializeField] TextMeshProUGUI eventText;
    [SerializeField] TextMeshProUGUI timerText;

    [SyncVar(hook = nameof(syncText))] string eventTextSync;
    [SyncVar(hook = nameof(syncTimer))] string timerTextSync;
    [SyncVar(hook = nameof(syncColor))] Color colorTextSync;

    Vector3 lastWaterPos;
    Vector3 lastBoatPos;
    Quaternion lastBoatRot;
    float yDiff;

    private void Awake()
    {
        eventText.gameObject.SetActive(true);
    }


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
    [Server]
    void setPlayerSpawn()
    {
        scoreKeeper.setPlayerSpawnPositions(playerSpawnPositions[eventIndex]);
        spawnPointsSet = true;
    }

    [Server]
    void triggerEvent(LevelEvents.VolcanoLevelEvent levelEvent)
    {
        print("triggering event: " + (eventIndex) + " at time: " + GameStats.RoundTimer);
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
    [Server]
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

    float nextEventCountdown = 0;

    [Server]
    void Update()
    {

        if (eventIndex < events.VolcanoLevelEvents.Length)
        {
            nextEvent = events.VolcanoLevelEvents[eventIndex];
            nextEventCountdown = nextEvent.startTime - GameStats.RoundTimer;

            if (nextEventCountdown < 5 && !spawnPointsSet)
            {
                setPlayerSpawn();
            }

            if (nextEventCountdown < 0 && !runningEvent)
            {
                triggerEvent(events.VolcanoLevelEvents[eventIndex]);
            }

            setCountdownText();
        }
        else
        {
            eventText.text = "";
            timerText.text = "";
        }


        if (0 < eventIndex)
        {
            LevelEvents.VolcanoLevelEvent currentEvent = events.VolcanoLevelEvents[eventIndex - 1];
            if (currentEvent.startTime < GameStats.RoundTimer)
            {
                lerpVal = (GameStats.RoundTimer - currentEvent.startTime) / currentEvent.runTime;
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

    [Server]
    void setCountdownText()
    {
        float minutes = Mathf.Floor(nextEventCountdown / 60);
        float seconds = Mathf.Floor(nextEventCountdown % 60);

        string min = minutes.ToString();
        string sec = seconds.ToString();

        if (minutes < 10)
        {
            min = "0" + minutes.ToString();
        }
        if (seconds < 10)
        {
            sec = "0" + Mathf.RoundToInt(seconds).ToString();
        }

        if (minutes < 1 && seconds < 10)
        {
            timerText.color = Color.red;

            if (seconds < 6 && nextEventCountdown < seconds + 0.5f)
            {
                timerText.color -= new Color(0,0,0,1);
            }
        }
        else
        {
            timerText.color = Color.white;
        }

        if (runningEvent)
        {
            eventText.text = "";
            timerText.text = "";
        }
        else
        {
            eventText.text = "Next Event: ";
            timerText.text = min + ":" + sec;
        }

        colorTextSync = timerText.color;
        eventTextSync = eventText.text;
        timerTextSync = timerText.text;

    }

    [Client]
    void syncText(string previous, string current)
    {
        eventText.text  = current;
    }
    [Client]
    void syncTimer(string previous, string current)
    {
        timerText.text = current;
    }
    [Client]
    void syncColor(Color previous, Color current)
    {
        timerText.color = current;
    }
}
