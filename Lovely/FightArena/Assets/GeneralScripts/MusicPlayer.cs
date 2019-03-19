using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class MusicPlayer : MonoBehaviour
{
    //fortesting
    [SerializeField]
    private bool update = true;

    [SerializeField]
    private AudioClip audioClip;
    [ShowOnly]
    [SerializeField]
    private int playTime = 0;
    [SerializeField]
    private Vector3 influenceOffset = Vector3.zero;
    [SerializeField]
    private Vector3 influenceRadius = Vector3.one;
    [ShowOnly]
    [SerializeField]
    private List<AudioSpeaker> currentSpeakers = new List<AudioSpeaker>();
    private Bounds influenceBounds = new Bounds();

    AudioSource inner;

    public delegate void AudioClipChangedEventHandler(MusicPlayer sender, AudioClipChangedEventArgs e);
    public delegate void SynchAudioTimeEventHandler(MusicPlayer sender, SynchTimeEventArgs e);
    public event AudioClipChangedEventHandler AudioClipChangedEvent;
    public event SynchAudioTimeEventHandler SynchAudioTimeEvent;

    private void Awake()
    {
        influenceBounds = new Bounds(transform.position, influenceRadius);
        ChangeExtents(influenceRadius);
        inner = GetComponent<AudioSource>();
        if (inner == null) inner = gameObject.AddComponent<AudioSource>();
        inner.clip = audioClip;
        inner.Play();
        inner.volume = 0;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if(transform.hasChanged || update)
        {
            transform.hasChanged = false;
            ChangeExtents(influenceRadius);
            update = false;
        }


        playTime = inner.timeSamples;
        OnSynchTime();
	}

    public void ChangeExtents(Vector3 newExtents)
    {
        influenceBounds.center = transform.position + influenceOffset;
        influenceBounds.extents = influenceRadius;
        foreach (var speaker in currentSpeakers)
        {
            speaker.Stop();
        }
        currentSpeakers.Clear();
        foreach (var speaker in AudioSpeaker.GetAudioSpeakersInRange(influenceBounds))
        {
            currentSpeakers.Add(speaker);
            speaker.SetInput(this, audioClip, playTime);
        }
    }

    public void OnSynchTime()
    {
        if (SynchAudioTimeEvent != null)
            SynchAudioTimeEvent(this, new SynchTimeEventArgs(playTime));
    }

    public void ChangeAudioClipPlaylist()
    {

    }

    private void OnDrawGizmosSelected()
    {
        ChangeExtents(influenceRadius);
        Gizmos.DrawWireCube(influenceBounds.center, influenceBounds.extents);
    }
}

public class SynchTimeEventArgs : EventArgs
{
    public readonly int newTime;

    public SynchTimeEventArgs(int newTime)
    {
        this.newTime = newTime;
    }
}
public class AudioClipChangedEventArgs : EventArgs
{
    public readonly AudioClip newClip;
    public readonly int newTime;

    public AudioClipChangedEventArgs(AudioClip newClip, int newTime)
    {
        this.newClip = newClip;
        this.newTime = newTime;
    }
}
