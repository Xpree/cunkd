using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] Vector3[] stops;
    [SerializeField] float stopTime;

    [SerializeField] bool loopStops;
    [SerializeField] bool colliderActivated;

    int currentStop = 0;
    int nextStop = 1;


    float atNextStop = 0;

    int passengers = 1;
    private void Awake()
    {
        moving = true;
        if (colliderActivated)
        {
            passengers = 0;
        }
    }

    int direction =1;
    private void MoveToNextStop()
    {
        float distanceFactor = (stops[nextStop] - stops[currentStop]).magnitude; 
        atNextStop = Mathf.Min(atNextStop += Time.fixedDeltaTime * speed/ distanceFactor, 1);

        transform.position = Vector3.Lerp(stops[currentStop], stops[nextStop], atNextStop);

        //arrived at stop
        if (atNextStop == 1)
        {
            moving = false;
            atNextStop = 0;
            timeWaited = 0;
            currentStop = nextStop;

            calculateNextStop();
        }
    }

    void calculateNextStop()
    {
        if (loopStops)
        {
            nextStop++;
            if (stops.Length <= nextStop)
            {
                nextStop = 0;
            }
        }
        else
        {
            nextStop += direction;
            if (stops.Length <= nextStop)
            {
                nextStop = currentStop - 1;
                direction = -1;
            }
            if (nextStop < 0)
            {
                nextStop = currentStop + 1;
                direction = 1;
            }
        }
    }

    float timeWaited;
    void waitAtStop()
    {
        timeWaited += Time.fixedDeltaTime;
        if (0 < passengers && stopTime <= timeWaited)
        {
            moving = true;
        }
    }

    bool moving = false;
    private void FixedUpdate()
    {
        if (moving)
        {
            MoveToNextStop();
        }
        else
        {
            waitAtStop();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (colliderActivated)
        {
            passengers++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (colliderActivated)
        {
            passengers--;
        }
    }
}
