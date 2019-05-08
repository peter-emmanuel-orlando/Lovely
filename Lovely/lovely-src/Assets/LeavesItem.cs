using System;

public class LeavesItem : Item, ILeaves
{
    public override  Type ItemType => typeof(ILeaves);

    public override float Volume { get; protected set; } = 0.2f;

    public override  MatterPhase Phase => MatterPhase.Solid;

    public override float ValuePerVolume => 0.10f;
}
