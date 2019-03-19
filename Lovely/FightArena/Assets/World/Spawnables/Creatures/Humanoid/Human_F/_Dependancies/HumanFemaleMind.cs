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
        //Debug.LogWarning("HumanFemaleMind does not yet have any behaviors!");
        return new EmptyPerformable();
    }
}
