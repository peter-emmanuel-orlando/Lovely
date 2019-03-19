using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericBody : Body
{
    [SerializeField]
    float maxHealth = 100f;
    [SerializeField]
    float maxStamina = 100f;
    [SerializeField]
    Gender gender = Gender.Nongendered;
    [SerializeField]
    string prefabName = "";

    EmptyMind emptyMind;

    public override Mind Mind { get { return emptyMind; } }
    public override string PrefabName { get { return prefabName; } }
    public override Gender Gender { get { return gender; } }
    public override float MaxHealth { get { return maxHealth; } }
    public override float MaxStamina { get { return maxStamina; } }
    public override List<Ability> BodyAbilities { get { return new List<Ability>(); } }

    protected override void Awake()
    {
        emptyMind = new EmptyMind(this);
        base.Awake();
    }


}
