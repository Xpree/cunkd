using UnityEngine;

[System.Serializable]
public class CameraShake : System.ICloneable
{
    public Oscillation3 PositionOscillator;
    public Oscillation3 RotationOscillator;
    public Oscilliation FieldOfViewOscillator;

    public float Duration;
    public bool RandomTimeOffset = true;


    [HideInInspector]
    public NetworkTimer activationTimer;


    public bool IsActive => activationTimer.IsSet && activationTimer.Elapsed < Duration;

    public object Clone()
    {
        CameraShake clone = new CameraShake();
        
        clone.PositionOscillator = PositionOscillator;
        clone.RotationOscillator = RotationOscillator;
        clone.FieldOfViewOscillator = FieldOfViewOscillator;
        clone.Duration = Duration;
        clone.RandomTimeOffset = RandomTimeOffset;
        clone.activationTimer = activationTimer;
        
        return clone;
    }

    public void Initialize(NetworkTimer timer)
    {
        activationTimer = timer;
        if(RandomTimeOffset)
        {
            PositionOscillator.RandomizeTimeOffset();
            RotationOscillator.RandomizeTimeOffset();
            FieldOfViewOscillator.RandomizeTimeOffset();
        }
    }

    public ShakeSample Sample()
    {
        float time = (float)activationTimer.Elapsed;        
        if (time <= Duration)
        {
            return new ShakeSample()
            {
                Position = PositionOscillator.SampleVector3(time),
                Rotation = RotationOscillator.SampleQuaternion(time),
                FieldOfView = FieldOfViewOscillator.Sample(time)
            };
        }
        else
        {
            return new ShakeSample()
            {
                Position = Vector3.zero,
                Rotation = Quaternion.identity,
                FieldOfView = 0
            };
        }
    }
}

[System.Serializable]
public struct ShakeSample
{
    public Vector3 Position;
    public Quaternion Rotation;
    public float FieldOfView;


    public override string ToString()
    {
        return $"Position: {Position}, Rotation: {Rotation.eulerAngles}, FieldOfView: {FieldOfView}";
    }
}

[System.Serializable]
public struct Oscilliation
{
    public float frequency;
    public float amplitude;
    public float timeOffset;


    public void RandomizeTimeOffset()
    {
        timeOffset = Random.Range(-Mathf.PI, Mathf.PI);
    }

    public float Sample(float time)
    {
        return amplitude * Mathf.Sin(frequency * (time + timeOffset));
    }
}

[System.Serializable]
public struct Oscillation3
{
    public Oscilliation x;
    public Oscilliation y;
    public Oscilliation z;

    public void RandomizeTimeOffset()
    {
        x.RandomizeTimeOffset();
        y.RandomizeTimeOffset();
        z.RandomizeTimeOffset();
    }

    public Vector3 SampleVector3(float time)
    {
        return new Vector3(x.Sample(time), y.Sample(time), z.Sample(time));
    }

    public Quaternion SampleQuaternion(float time)
    {
        return Quaternion.Euler(x.Sample(time), y.Sample(time), z.Sample(time));
    }
}

