using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Static game settings that wont be changed by the player
[CreateAssetMenu(fileName = "GameSettings", menuName = "Scriptable Objects/Game Settings")]
public class GameSettings : ScriptableObject
{
    public float MaxSpeed = 9.0f;
    public float DecelerationSpeed = 27f;
    public float JumpHeight = 2.0f;
    public float AirMovementMultiplier = 1.0f;

    public double CoyoteTime = 1.0;
    public double StrongAirControlTime = 0.1;
}
