using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Star : MonoBehaviour, lItem
{
    public static event Action<int> OnStarCollect;
    public int worth = 5;

    public void Collect()
    {
        OnStarCollect?.Invoke(worth);
        SoundEffectManager.Play("Star");
        Destroy(gameObject);
    }
}