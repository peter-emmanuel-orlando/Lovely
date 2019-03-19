using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ScheduledMessageAnimation
{
    public readonly AnimationClip animation;
    public readonly Queue<ScheduledMessage> scheduledMessages = new Queue<ScheduledMessage>();
    public readonly string abortMessage = StdAnimMsg.aborted;

    public ScheduledMessageAnimation(AnimationClip animation, string abortMessage = StdAnimMsg.aborted, params ScheduledMessage[] scheduledMessages)
    {

        if (animation == null) throw new System.ArgumentNullException();
        foreach (var item in scheduledMessages)
        {
             if( scheduledMessages == null) throw new System.ArgumentNullException();
        }

        this.animation = animation;
        var tmp = new List<ScheduledMessage>(scheduledMessages);
        tmp.Sort();
        this.scheduledMessages = new Queue<ScheduledMessage>(tmp);

        this.abortMessage = abortMessage;
    }
}

public static class StdAnimMsg
{
    public const string activeFramesStart = "activeFramesStart";
    public const string activeFramesEnd = "activeFramesEnd";
    public const string lockInAnimStart = "lockInAnimStart";
    public const string lockInAnimEnd = "lockInAnimEnd";
    public const string aborted = "aborted";
}

public class ScheduledMessage : System.IComparable<ScheduledMessage>
{
    public readonly string message;
    public readonly float triggerTimeNormalized;

    public ScheduledMessage(string message, float triggerTimeNormalized)
    {
        this.message = message;
        this.triggerTimeNormalized = triggerTimeNormalized;
    }

    public int CompareTo(ScheduledMessage other)
    {
        return this.triggerTimeNormalized.CompareTo(other.triggerTimeNormalized);
    }
}