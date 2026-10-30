using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedItem : MonoBehaviour, lItem
{
    public static event Action<float> OnSpeedCollected;
    public float speedBoost = 1.5f;

    public void Collect()
    {
        OnSpeedCollected?.Invoke(speedBoost);
        SoundEffectManager.Play("Boots");
        Destroy(gameObject);
    }
}
