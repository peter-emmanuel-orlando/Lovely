using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AnimationEventMessages
{
    public const string receiveAnimationFunctionName = "ReceiveAnimationEvents";

    public const string activeFramesStart = "ActiveFrames:Start";
    public const string activeFramesEnd = "ActiveFrames:End";
    public const string animationLock = "AnimationLock:Lock";
    public const string animationUnlock = "AnimationLock:Unock";

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
}
