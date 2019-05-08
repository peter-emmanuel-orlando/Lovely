using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FairieFireMind : PerceivingMind
{
    private readonly WanderPerformable wander;

    public override float SightRadius { get { return 10f; } }

    public FairieFireMind(FairieFireBody body) : base(body)
    {
            wander = new WanderPerformable(this);
    }

    public override IPerformable GetDecisions()
    {
        return wander;
    }
}
