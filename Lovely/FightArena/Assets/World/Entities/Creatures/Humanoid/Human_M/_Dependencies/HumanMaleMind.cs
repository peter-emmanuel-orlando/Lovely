using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanMaleMind : Mind
{
    public HumanMaleMind(Body body) : base(body)
    {

    }

    protected override float SightRange { get { return 60f; } }

    public override IPerformable GetDecisions()
    {
        Debug.LogWarning("HumanMaleMind does not yet have any behaviors!");
        return EmptyPerformable.empty;
    }
}
