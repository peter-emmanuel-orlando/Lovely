﻿using System;

public class LeavesItem : Item, ILeaves
{
    public override  Type ItemType => typeof(ILeaves);

    public override float Volume => 0.2f;

    public override  MatterPhase Phase => MatterPhase.Solid;
}