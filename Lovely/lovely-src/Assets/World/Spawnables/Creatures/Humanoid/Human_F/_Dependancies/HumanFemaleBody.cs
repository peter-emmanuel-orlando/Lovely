using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanFemaleBody : HumanBody
{
    HumanFemaleMind femaleMind;
    private readonly List<Ability> femaleBodyAbilities = new List<Ability>();

    public override PerceivingMind Mind { get { return femaleMind; } }
    public override string PrefabName { get { return "HumanFemale"; } }
    public override Gender Gender { get { return Gender.Female; } }

    protected override void Awake()
    {
        femaleMind = new HumanFemaleMind(this);
        base.Awake();
    }
}
