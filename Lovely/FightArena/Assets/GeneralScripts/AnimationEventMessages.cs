using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AnimationEventMessages
{
    public const string receiveAnimationFunctionName = "ReceiveAnimationEvents";

    public const string activeFramesStart = "ActiveFrames:Start";
    public const string activeFramesEnd = "ActiveFrames:End";
    public const string animationLock = "AnimationLock:Lock";
    public const string animationUnlock = "AnimationLock:Unlock";

    public static AnimationEvent GetEvent(float time, string stringParameter)
    {
        var result = new AnimationEvent();
        result.time = time;
        result.functionName = receiveAnimationFunctionName;
        result.stringParameter = stringParameter;
        return result;
    }

    public static AnimationEvent GetActiveFramesStartEvent(float time)
    {
        return GetEvent(time, activeFramesStart);
    }
    public static AnimationEvent GetActiveFramesEndEvent(float time)
    {
        return GetEvent(time, activeFramesEnd);
    }
    public static AnimationEvent GetAnimationLockEvent(float time)
    {
        return GetEvent(time, animationLock);
    }
    public static AnimationEvent GetAnimationUnlockEvent(float time)
    {
        return GetEvent(time, animationUnlock);
    }

    public static AnimationClip AddEventsToAnimation(this AnimationClip clip, params AnimationEvent[] animEvent)
    {
        foreach (var evnt in animEvent)
        {
            clip.AddEvent(evnt);
        }
        return clip;
    }

    public static AnimationClip AddEventsAtNormalizedTime(this AnimationClip clip, AnimationEvent[] animEvent, float[] normalizedTimes)
    {
        if (animEvent.Length != normalizedTimes.Length) throw new System.Exception("both arrays must have the same length!");
        for (int i = 0; i < animEvent.Length; i++)
        {
            animEvent[i].time = GetTimeFromNormalized(clip, normalizedTimes[i]);
            clip.AddEvent(animEvent[i]);
        }
        return clip;
    }

    public static float GetTimeFromNormalized(this AnimationClip clip, float normalizedTime)
    {
        return normalizedTime * clip.length;
    }
}

public struct AnimationMessage : System.IComparable<AnimationMessage>
{
    public readonly bool isValid;
    public readonly string message;
    public readonly float triggerTimeNormalized;

    public AnimationMessage(string message, float triggerTimeNormalized)
    {
        isValid = true;
        this.message = message;
        this.triggerTimeNormalized = triggerTimeNormalized;
    }

    public int CompareTo(AnimationMessage other)
    {
        return this.triggerTimeNormalized.CompareTo(other.triggerTimeNormalized);
    }
}