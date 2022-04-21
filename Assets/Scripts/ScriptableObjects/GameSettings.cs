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
    public class GravityGunSettings
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
    
    public GravityGunSettings GravityGun = new();



    [Serializable]
    public class BlackHoleGunSettings
    {
        public float Cooldown = 30f;
        public float Range = 20f;
    }

    public BlackHoleGunSettings BlackHoleGun = new();

    [Serializable]
    public class BlackHoleSettings
    {
        public float Range = 20f;
        public float Duration = 5f;
        public float Intensity = 1f;
    }

    public BlackHoleSettings BlackHole = new();

    [Serializable]
    public class IceGadgetSettings
    {
        public float Duration = 30f;
        public float Friction = -0.5f;
    }

    public IceGadgetSettings IceGadget = new();

}
