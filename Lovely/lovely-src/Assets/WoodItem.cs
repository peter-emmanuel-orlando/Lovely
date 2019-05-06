using System;

public class WoodItem : Item, IWood
{
    public override Type ItemType => typeof(IWood);

    public override float Volume => 0.2f;

    public override MatterPhase Phase => MatterPhase.Solid;
}
