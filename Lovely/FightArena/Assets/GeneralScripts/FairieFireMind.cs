using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FairieFireMind : Mind
{
    private readonly WanderPerformable wander;

    protected override float SightRange { get { return 10f; } }

    public FairieFireMind(FairieFireBody body) : base(body)
    {
            wander = new WanderPerformable(this);
    }

    public override IPerformable GetDecisions()
    {
        return wander;
    }
}
