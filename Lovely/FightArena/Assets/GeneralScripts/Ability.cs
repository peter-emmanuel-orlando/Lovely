﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public abstract class Ability
{
    public abstract float Range { get; }

    protected readonly UnifiedController uniControl;

    public Ability(UpdateSubscriber SubscribeForUpdate, TriggerEventSubscriber SubscribeForTriggerEvents, UnifiedController uniControl)
    {
        SubscribeForUpdate(Update);
        SubscribeForTriggerEvents(ReceiveTriggerEvents);
        this.uniControl = uniControl;
    }


    public abstract ProgressStatus CheckStatus();
    
    public abstract IEnumerator<ProgressStatus> CastAbility();

    protected abstract void ReceiveTriggerEvents(Collider collider);

    protected abstract void Update();

    
}

/*
 * abilities have zones and castTime.
 * zones is an idea expressing the strategic useful range of the ability. If an enemy is
 * within the min to max distance of the caster the idea is that the ability is useful.
 * Zones is based on cast time and range. range determines the outer useful distance, cast
 * time determines the minimum distance
 * 
 * 
 * 
 * 
 * 
 */

