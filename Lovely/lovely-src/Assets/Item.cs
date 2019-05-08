using System;

public abstract class Item : IItem
{
    public abstract Type ItemType { get; }
    public abstract float Volume { get; protected set; }
    public abstract float ValuePerVolume { get; }
    public float Value => ValuePerVolume * Volume;
    public bool IsEmpty { get { return Volume <= 0; } }
    public abstract MatterPhase Phase { get; }

    public void UseItem()
    {
        Volume = 0;
    }

    public virtual bool UseVolume(float volume)
    {
        var success = Volume <= volume;
        if(success)
            Volume -= volume;
        return success;
    }
}
