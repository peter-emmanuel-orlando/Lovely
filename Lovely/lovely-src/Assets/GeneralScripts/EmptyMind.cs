using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyMind : PerceivingMind
{

    public EmptyMind(Body body) : base(body)
    {

    }
    
    public override float SightRadius { get { return 10f;} }

    public override IPerformable GetDecisions()
    {
        return new EmptyPerformable();
    }
}
