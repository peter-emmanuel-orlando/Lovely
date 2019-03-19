using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SingleAudioListner
{
    private static AudioListener singleListener;

    public static AudioListener AttachAudioListner(GameObject receiver)
    {
        GameObject.Destroy(singleListener);
        singleListener = receiver.AddComponent<AudioListener>();
        return singleListener;
    }
}
