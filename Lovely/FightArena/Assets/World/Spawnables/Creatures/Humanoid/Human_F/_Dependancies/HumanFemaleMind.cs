using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanFemaleMind : HumanMind
{

    public HumanFemaleMind(Body body) : base(body)
    {

    }

    protected override float SightRange { get { return 60f; } }

    public override IPerformable GetDecisions()
    {
        Debug.LogWarning("HumanFemaleMind does not yet have any behaviors!");
        return EmptyPerformable.empty;
    }
}
