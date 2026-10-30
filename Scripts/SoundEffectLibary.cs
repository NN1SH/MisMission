using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectLibary : MonoBehaviour
{
    [SerializeField] private SoundEffectGroup[] soundEffectGroups;
    private Dictionary<string, List<AudioClip>> soundsDictionary;

    private void Awake()
    {
        InitialDictionary();
    }

    private void InitialDictionary()
    {
        soundsDictionary = new Dictionary<string, List<AudioClip>>();
        foreach(SoundEffectGroup soundEffectGroup in soundEffectGroups)
        {
            soundsDictionary[soundEffectGroup.name] = soundEffectGroup.audioClips;
        }
    }

    public AudioClip GetRandomClip(string name)
    {
        if (soundsDictionary.ContainsKey(name))
        {
            List<AudioClip> audioClips = soundsDictionary[name];
            if(audioClips.Count > 0)
            {
                return audioClips[UnityEngine.Random.Range(0, audioClips.Count)];
            }
        }
        return null;
        
    }

}
[System.Serializable]

public struct SoundEffectGroup
{
    public string name;
    public List<AudioClip> audioClips;
}
