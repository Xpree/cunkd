using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VFXAnimator : MonoBehaviour
{

    [System.Serializable]
    public class NamedEffect
    {
        public string Name;
        public VisualEffect VFX;
    }

    public NamedEffect[] Effects;

    public void PlayEffect(string name)
    {
        if (Effects == null || Effects.Length == 0)
        {
            return;
        }

        foreach (var effect in Effects)
        {
            if (effect.Name == name)
            {
                effect.VFX.Play();
            }
        }

    }

    public void StopEffect(string name)
    {
        if (Effects == null || Effects.Length == 0)
        {
            return;
        }

        foreach (var effect in Effects)
        {
            if (effect.Name == name)
            {
                effect.VFX.Stop();
            }
        }

    }
}
