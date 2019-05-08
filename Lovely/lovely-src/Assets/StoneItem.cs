using System;

public class StoneItem : Item, IStone
{
    public override Type ItemType => typeof(IStone);

    /// <summary>
    /// cu meter
    /// </summary>
    public override float Volume { get; protected set; } = 0.2f;

    public override MatterPhase Phase => MatterPhase.Solid;
    /// <summary>
    /// $/m^3
    /// </summary>
    public override float ValuePerVolume => 30;
}
