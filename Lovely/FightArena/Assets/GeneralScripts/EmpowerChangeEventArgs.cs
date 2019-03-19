using System;


public class EmpowerChangeEventArgs : EventArgs
{
    public readonly int oldPowerLevel;
    public readonly int newPowerLevel;

    public EmpowerChangeEventArgs(int oldPowerLevel, int newPowerLevel)
    {
        this.oldPowerLevel = oldPowerLevel;
        this.newPowerLevel = newPowerLevel;
    }
}
