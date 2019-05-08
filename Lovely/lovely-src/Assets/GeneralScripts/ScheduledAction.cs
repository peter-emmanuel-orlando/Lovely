using System;
using System.Collections.Generic;

public class ScheduledActionQueue
{
    private readonly ScheduledAction[] inner;

    public ScheduledActionQueue()
    { }

    public ScheduledActionQueue(params ScheduledAction[] scheduledActions)
    {
        Array.Sort(scheduledActions);
        inner = scheduledActions;
    }

    public ScheduledActionQueue(ScheduledAction[] scheduledActions, params ScheduledAction[] additionalActions)
    { 
        var newInner = new List<ScheduledAction>();
        if (scheduledActions != null)
            newInner.AddRange(scheduledActions);
        newInner.AddRange(additionalActions);
        newInner.Sort();
        inner = newInner.ToArray();
    }

    public Queue<ScheduledAction> GetCopyOfQueue()
    {
        return new Queue<ScheduledAction>(inner);
    }
    public Queue<ScheduledAction> GetCopyOfQueue(params ScheduledAction[] withAdditionalActions)
    {
        var l = ToArray(withAdditionalActions);
        return new Queue<ScheduledAction>(l);
    }
    public ScheduledAction[] ToArray(params ScheduledAction[] withAdditionalActions)
    {
        var result = new List<ScheduledAction>();
        result.AddRange(inner);
        result.AddRange(withAdditionalActions);
        result.Sort();
        return result.ToArray();
    }
}

public struct ScheduledAction : IComparable<ScheduledAction>
{
    public readonly float time;
    //public readonly Guarentees guarentees;
    private readonly Action action;

    public ScheduledAction(float time, Action action)
    {
        if (action == null) throw new ArgumentNullException();
        this.time = time;
        this.action = action;
    }

    public void PerformAction()
    {
        action();
    }

    public int CompareTo(ScheduledAction other)
    {
        return this.time.CompareTo(other.time);
    }

    /*
    public ScheduledAction(float time, Action action) : this(time, Guarentees.None, action)
    {        }

    public ScheduledAction(float time, Guarentees options, Action action)
    {
        if (action == null) throw new ArgumentNullException();
        this.time = time;
        this.guarentees = options;
        this.action = action;
    }

    public enum Guarentees
    {
        None = 0,
        GuarenteeExecution,
        GuarenteeFrame
    }
    */
}

