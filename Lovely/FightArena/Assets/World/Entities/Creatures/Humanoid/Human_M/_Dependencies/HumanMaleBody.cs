using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanMaleBody : Body
{
    HumanMaleMind maleMind;

    public override Mind Mind { get { return maleMind; } }
    public override string PrefabName { get { return "HumanMale"; } }
    public override Gender Gender { get { return Gender.Male; } }
    public override float MaxHealth { get { return 100f; } }
    public override float MaxStamina { get { return 100f; } }

    protected override void Awake()
    {
        maleMind = new HumanMaleMind(this);
        base.Awake();
    }
}
