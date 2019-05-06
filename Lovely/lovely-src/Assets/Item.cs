using System;

public abstract class Item : IItem
{
    public abstract Type ItemType { get; }
    public abstract float Volume { get; }
    public abstract MatterPhase Phase { get; }
}
