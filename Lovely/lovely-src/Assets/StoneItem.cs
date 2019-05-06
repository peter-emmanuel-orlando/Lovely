using System;

public class StoneItem : Item, IStone
{
    public override Type ItemType => typeof(IStone);

    public override float Volume => 0.2f;

    public override MatterPhase Phase => MatterPhase.Solid;
}
