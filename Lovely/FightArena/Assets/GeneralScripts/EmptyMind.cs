using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyMind : Mind
{

    public EmptyMind(Body body) : base(body)
    {

    }

    public override List<Ability> MindAbilities { get { return new List<Ability>(); } }

    protected override float SightRange { get { return 10f;} }

    public override IPerformable GetDecisions()
    {
        return EmptyPerformable.empty;
    }
}
