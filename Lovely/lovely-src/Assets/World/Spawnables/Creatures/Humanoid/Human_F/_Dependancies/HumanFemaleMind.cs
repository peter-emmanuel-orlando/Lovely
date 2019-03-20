using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanFemaleMind : HumanMind
{

    public HumanFemaleMind(Body body) : base(body)
    {

    }

    public override float SightRadius { get { return 60f; } }

    public override IPerformable GetDecisions()
    {
        return base.GetDecisions();//new EmptyPerformable();
    }
}
