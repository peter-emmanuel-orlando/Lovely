using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanFemaleBody : Body
{
    HumanFemaleMind femaleMind;

    public override Mind Mind { get { return femaleMind; } }
    public override string PrefabName { get { return "HumanFemale"; } }
    public override Gender Gender { get { return Gender.Female; } }
    public override float MaxHealth { get { return 100f; } }
    public override float MaxStamina { get { return 100f; } }

    protected override void Awake()
    {
        femaleMind = new HumanFemaleMind(this);
        base.Awake();
    }
}
