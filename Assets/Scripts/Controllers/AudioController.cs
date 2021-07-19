using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField]
    public CustomAudio[] audioClips;

    public static AudioController instance; //singleton
    public float Volume
    {
        get
        {
            return Volume;
        }
        set
        {
            Volume = Mathf.Clamp01(value);
            if (currentlyPlaying != null)
            {
                currentlyPlaying.source.volume = Volume;
            }
        }
    }

    private CustomAudio? currentlyPlaying = null;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        foreach (CustomAudio s in audioClips)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.audioClip;
            s.source.loop = s.shouldLoop;
        }
    }

    public void PlaySound(string name)
    {
        CustomAudio s = System.Array.Find(audioClips, (customSound) => customSound.name == name);

        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        s.source.volume = s.volume;
        s.source.pitch = s.pitch;
        s.source.time = s.timeToStart;

        s.source.Play();
        currentlyPlaying = s;
    }

    public void StopSound(string name)
    {
        CustomAudio s = System.Array.Find(audioClips, (customSound) => customSound.name == name);

        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        s.source.Stop();
        currentlyPlaying = null;
    }
}

[System.Serializable]
public class CustomAudio
{
    public string name;
    public AudioClip audioClip;
    public bool shouldLoop = false;
    [Range(0, 1)]
    public float volume = 1;
    [Range(0.1f, 3)]
    public float pitch = 1;
    public float timeToStart = 0f;

    [HideInInspector]
    public AudioSource source;
}