using System;

public struct ScheduledAction
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

