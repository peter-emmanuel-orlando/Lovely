using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class HumanBody : HumanoidBody
{
    protected override void Awake()
    {
        BloodMax = 100;
        CaloriesMax = 4;
        StaminaMax = 100;
        base.Awake();
        CharacterAbilities[CharacterAbilitySlot.BasicPunchCombo] = new Punch(this);
        CharacterAbilities[CharacterAbilitySlot.DashPunch] = new DashPunch(this);
        CharacterAbilities[CharacterAbilitySlot.ThrowItem] = new AzuriteDartAttack(this);
        CharacterAbilities[CharacterAbilitySlot.RangedPower] = new BloodNovaBeamAttack(this);
        CharacterAbilities[CharacterAbilitySlot.Dodge] = new Dodge(this);
        CharacterAbilities[CharacterAbilitySlot.Block] = new Block(this);

    }
}
