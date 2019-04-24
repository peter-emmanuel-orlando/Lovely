using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericBody : Body
{
    [SerializeField]
    Gender gender = Gender.Nongendered;
    [SerializeField]
    string prefabName = "";

    EmptyMind emptyMind;

    public override PerceivingMind Mind { get { return emptyMind; } }
    public override string PrefabName { get { return prefabName; } }
    public override Gender Gender { get { return gender; } }

    protected override void Awake()
    {
        emptyMind = new EmptyMind(this);        
        base.Awake();
    }


}
