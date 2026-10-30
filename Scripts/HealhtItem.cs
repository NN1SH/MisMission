using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealhtItem : MonoBehaviour, lItem
{
    public int healAmount = 1;
    public static event Action<int> OnHealthCollected;
    public void Collect()
    {
        OnHealthCollected?.Invoke(healAmount);
        SoundEffectManager.Play("Health");
        Destroy(gameObject);
    }
}
