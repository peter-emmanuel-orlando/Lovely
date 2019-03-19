using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class HumanBody : Body
{
    private readonly CharacterAbilities characterAbilities = new CharacterAbilities();
    public override CharacterAbilities CharacterAbilities { get { return characterAbilities; } }


    protected override void Awake()
    {
        base.Awake();
        characterAbilities[CharacterAbilitySlot.BasicPunchCombo] = new Punch(this);
        characterAbilities[CharacterAbilitySlot.DashPunch] = new DashPunch(this);
        characterAbilities[CharacterAbilitySlot.ThrowItem] = new AzuriteDartAttack(this);
        characterAbilities[CharacterAbilitySlot.RangedPower] = new BloodNovaBeamAttack(this);
        characterAbilities[CharacterAbilitySlot.Block] = new Block(this);
    }
}
