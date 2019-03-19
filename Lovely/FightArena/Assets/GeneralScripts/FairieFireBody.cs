using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FairieFireBody : Body, IBodyCanGlow
{
    Light lightSource;
    private FairieFireMind fairieFireMind;



    public override Mind Mind { get { return fairieFireMind; } }
    public override string PrefabName { get { return "FairieFire"; } }
    public override Gender Gender { get { return Gender.Nongendered; } }
    public override float MaxHealth { get { return 50f; } }
    public override float MaxStamina { get { return float.MaxValue; } }
    public override List<Ability> BodyAbilities { get { return new List<Ability>(); } }

    public Color GlowColor
    {
        get { return lightSource.color; }
        set { lightSource.color = value; }
    }

    protected override void Awake()
    {
        fairieFireMind = new FairieFireMind(this);
        base.Awake();
        lightSource = GetComponentInChildren<Light>();
        if (lightSource == null)
            throw new UnityException("FairieFire needs a light component in heiarchy!");

    }
}
