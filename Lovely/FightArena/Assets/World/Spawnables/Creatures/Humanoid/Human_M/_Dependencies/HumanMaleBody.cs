using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanMaleBody : HumanBody
{
    HumanMaleMind maleMind;
    private readonly List<Ability> maleBodyAbilities = new List<Ability>();
     
    public override Mind Mind { get { return maleMind; } }
    public override string PrefabName { get { return "HumanMale"; } }
    public override Gender Gender { get { return Gender.Male; } }

    protected override void Awake()
    {
        maleMind = new HumanMaleMind(this);
        base.Awake();
        BloodMax = 100;
        StaminaMax = 100;
    }
}
