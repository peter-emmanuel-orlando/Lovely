using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSpeaker : MonoBehaviour
{
    private static readonly HashSet<AudioSpeaker> allSpeakers = new HashSet<AudioSpeaker>();
    public static AudioSpeaker[] GetAudioSpeakersInRange(Bounds range)
    {
        var result = new List<AudioSpeaker>();
        foreach (var speaker in allSpeakers)
        {
            if(range.Contains(speaker.transform.position))
                result.Add(speaker);
        }
        return result.ToArray();
    }


    [ShowOnly]
    private MusicPlayer input;

    [ShowOnly]
    private AudioSource output;

    private void OnEnable()
    {
        output = GetComponentInChildren<AudioSource>();
        if (output == null) output = gameObject.AddComponent<AudioSource>();
        output.playOnAwake = false;
        output.spatialBlend = 1f;
        output.dopplerLevel = 0f;

        if (!allSpeakers.Contains(this))
            allSpeakers.Add(this);

        if (input != null)
        {
            input.AudioClipChangedEvent += ReceiveAudioClipChangeEvent;
            input.SynchAudioTimeEvent += ReceiveSynchAudioTimeEvent;
        }
    }

    public void SetInput(MusicPlayer musicPlayer, AudioClip audioClip, float time )
    {
        if (input != null)
        {
            input.AudioClipChangedEvent -= ReceiveAudioClipChangeEvent;
            input.SynchAudioTimeEvent -= ReceiveSynchAudioTimeEvent;
        }
        input = musicPlayer;
        input.AudioClipChangedEvent += ReceiveAudioClipChangeEvent;
        input.SynchAudioTimeEvent += ReceiveSynchAudioTimeEvent;

        output.clip = audioClip;
        output.Play();
        output.time = time;
    }

    public void Play(float time)
    {
        output.Play();
        output.time = time;
    }

    public void Stop()
    {
        output.Stop();
    }

    private void ReceiveAudioClipChangeEvent(MusicPlayer source, AudioClipChangedEventArgs e)
    {
        output.Stop();
        output.clip = e.newClip;
        output.Play();
        output.time = e.newTime;
    }

    private void ReceiveSynchAudioTimeEvent(MusicPlayer source, SynchTimeEventArgs e)
    {
        if(e.newTime > 0 && e.newTime < output.clip.samples)
            output.timeSamples = e.newTime;
    }

    private void OnDisable()
    {
        if (allSpeakers.Contains(this))
            allSpeakers.Remove(this);

        if (input != null)
        {
            input.AudioClipChangedEvent -= ReceiveAudioClipChangeEvent;
            input.SynchAudioTimeEvent -= ReceiveSynchAudioTimeEvent;
        }
    }

}
