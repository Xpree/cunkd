using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Static game settings that wont be changed by the player
[CreateAssetMenu(fileName = "GameSettings", menuName = "Scriptable Objects/Game Settings")]
public class GameSettings : ScriptableObject
{
    [Serializable]
    public class CharacterMovementSettings
    {
        public float MaxSpeed = 9.0f;
        public float DecelerationSpeed = 27f;
        public float JumpHeight = 2.0f;
        public double CoyoteTime = 1.0;
        public float AirMovementMultiplier = 1.0f;
        public double StrongAirControlTime = 0.1;
    }

    public CharacterMovementSettings CharacterMovement = new();

    [Serializable]
    public class GravitygunSettings
    {
        //grab
        public float MaxGrabRange = 40f;
        public float GrabTime = 0.5f;
        public float GrabTorque = 10f;

        //push
        public float MinPushForce = 10f;
        public float MaxPushForce = 100f;
        public float ChargeRate = 1f;
        public float MaxRange = 30f;

    }

    
    public GravitygunSettings GravityGun = new();

}
