using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class HumanMind : Mind
{
    public override List<Ability> MindAbilities { get { return new List<Ability>(); } }

    public HumanMind(Body body) : base(body)
    {

    }
}
