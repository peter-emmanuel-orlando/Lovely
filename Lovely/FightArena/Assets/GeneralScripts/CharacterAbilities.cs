using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAbilities
{
    private readonly Dictionary<CharacterAbilitySlot, Ability> inner = new Dictionary<CharacterAbilitySlot, Ability>();

    public CharacterAbilities()
    {
        foreach (var item in Enum.GetValues(typeof(CharacterAbilitySlot)))
        {
            var castItem = (CharacterAbilitySlot)item;
            inner.Add(castItem, null);
        }
    }
    
    public Ability this[CharacterAbilitySlot abilitySlot]
    {
        get { return inner[abilitySlot]; }
        set { inner[abilitySlot] = value; }
    }
}

public enum CharacterAbilitySlot
{
    None = 0,
    SuperJump,
    Fly,
    BasicPunchCombo,
    DashPunch,
    ThrowItem,
    RangedPower,
    Block,
    Dodge,
    Meditate,
    SpecialAttack,
    UltimateAttack,

    //everyday abilities
    //LayDown
    //Sit,
    //Eat,
    //Harvest
}
