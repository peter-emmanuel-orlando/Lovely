using System;
using UnityEngine;

public delegate void EmpowerChangeEventHandler(Body sender, EmpowerChangeEventArgs e);

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

public delegate void DamageTakenEventHandler(Body sender, DamageTakenEventArgs e);

public class DamageTakenEventArgs : EventArgs
{
    public readonly int oldPowerLevel;
    public readonly int newPowerLevel;

    public DamageTakenEventArgs(int oldPowerLevel, int newPowerLevel)
    {
        this.oldPowerLevel = oldPowerLevel;
        this.newPowerLevel = newPowerLevel;
    }
}

