using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanFemaleBody : HumanBody
{
    HumanFemaleMind femaleMind;
    private readonly List<Ability> femaleBodyAbilities = new List<Ability>();

    public override Mind Mind { get { return femaleMind; } }
    public override string PrefabName { get { return "HumanFemale"; } }
    public override Gender Gender { get { return Gender.Female; } }
    public override float MaxHealth { get { return 100f; } }
    public override float MaxStamina { get { return 100f; } }

    public override List<Ability> BodyAbilities
    {
        get
        {
            var result = new List<Ability>(femaleBodyAbilities);
            result.AddRange(base.BodyAbilities);
            return result;
        }
    }

    protected override void Awake()
    {
        femaleMind = new HumanFemaleMind(this);
        base.Awake();
    }
}
