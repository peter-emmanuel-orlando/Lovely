using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class HumanBody : Body
{
    private readonly List<Ability> bodyAbilities = new List<Ability>();    
    public override List<Ability> BodyAbilities { get { return new List<Ability>(bodyAbilities); } }

    protected override void Awake()
    {
        base.Awake();
        bodyAbilities.Add(new PhysicalAttack(this));
    }
}
