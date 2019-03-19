using System;
using UnityEngine;

public class TriggerEventArgs : EventArgs
{
    public readonly Collider collider;
    public TriggerEventArgs(Collider collider)
    { this.collider = collider; }
}
