using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyMind : Mind
{
    public EmptyMind(Body body) : base(body)
    {

    }

    public override IPerformable GetDecisions()
    {
        return EmptyPerformable.empty;
    }
}
