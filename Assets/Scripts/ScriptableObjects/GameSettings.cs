using System;
using UnityEngine;

// Static game settings that wont be changed by the player
[CreateAssetMenu(fileName = "GameSettings", menuName = "Scriptable Objects/Game Settings")]
public class GameSettings : ScriptableObject
{
    [Serializable]
    public class CharacterMovementSettings
    {
        public float TerminalVelocity = 1000.0f;
        public float MaxSpeed = 10.0f;
        public float JumpHeight = 2.0f;
        
        [Tooltip("How long a player is consider grounded after not being")]
        public double CoyoteTime = 1.0;
        [Tooltip("How long ground acceleration multiplier applies in air after a jump")]
        public double StrongAirControlTime = 0.1;

        public float GroundAcceleration = 10.0f;
        public float FrictionAcceleration = 6.0f;
        public float AirAcceleration = 1.0f;

        [Tooltip("Apply friction with atleast this speed")]
        public float FrictionMinSpeed = 3.0f;


        public float GroundedRadius = 0.28f;
        public float GroundedOffset = 0.16f;
        public LayerMask GroundLayers = ~0;
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

        public float MaxRange = 20f;
        public float Cooldown = 0f;
        public int Charges = 1;
    }

    public IceGadgetSettings IceGadget = new();

    [Serializable]
    public class SwapSniperSettings
    {
        public float Cooldown = 5f;
        public float Range = 200f;
    }

    public SwapSniperSettings SwapSniper = new();

    public LayerMask Movable;
    public LayerMask ProtectileTargetLayers;
    public float SmallSphereCastRadius = 0.25f;
    public float AutodespawnTimer = 30.0f;

    public float GlobalKnockbackMultiplier = 2.0f;
    public float GlobalKnockbackRamp = 1.0f;
    public float GlobalKnockupMultiplier = 1.0f;


    public LayerMask ShakeableLayer;
}

