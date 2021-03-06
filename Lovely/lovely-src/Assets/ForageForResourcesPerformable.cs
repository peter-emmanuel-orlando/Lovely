﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ForageForResourcesPerformable : Performable
{

    //change to each mindStat in in-game days. this multi-part performable returns the stats of the performables that make it up
    public override float DeltaWakefulness { get { return (currentPerformable != null)? currentPerformable.DeltaWakefulness : 0f; } }//decreases when awake, depletes quicker when performing taxing tasks, increases 1.5x speed when sleeping
    public override float DeltaExcitement { get { return (currentPerformable != null) ? currentPerformable.DeltaExcitement : 0f; } }//depletes awake or asleep. Tedious work takes chunks from this. relaxation activities increase this
    public override float DeltaSpirituality { get { return (currentPerformable != null) ? currentPerformable.DeltaSpirituality : 0f; } }//depletes awake or asleep. relaxation activities increase this. Tedious Work or seedy activities takes chunks from this
    public override float DeltaSocialization { get { return (currentPerformable != null) ? currentPerformable.DeltaSocialization : 0f; } }//depletes when awake. increases when working or playing together
    public override float DeltaCalories { get { return (currentPerformable != null) ? currentPerformable.DeltaCalories : 0f; } }//num days calories will last.
    public override float DeltaBlood { get { return (currentPerformable != null) ? currentPerformable.DeltaBlood : 0f; } }//reaching 0 blood and being passes out
    public override bool IsSleepActivity { get { return (currentPerformable != null) ? currentPerformable.IsSleepActivity : false; } }//is this activity sleeping?
    //*//////////////////////////////////////////////////////////////////////////////////////////////////

    public override ActivityState ActivityType { get { return ActivityState.Work; } }
    //dumb implementation, unoptomised. Nee do do a thing like in Being
    IEnumerable<Type> resourcesToSearchFor;
    float SearchRadius { get { return Performer.SightRadius; } }

    public ForageForResourcesPerformable(PerceivingMind performer, params TypeLimiter<IResource>[] resourcesToSearchFor) : base(performer)
    {
        this.resourcesToSearchFor = resourcesToSearchFor.Cast<Type>();
    }

    public ForageForResourcesPerformable(PerceivingMind performer) : base(performer)
    {
        this.resourcesToSearchFor = new Type[] { typeof(IResource) };
    }

    public void ChangeDesiredResources(List<TypeLimiter<IItem>> newResourcesToSearchFor)
    {
        this.resourcesToSearchFor = newResourcesToSearchFor.Cast<Type>();
    }

    IPerformable currentPerformable = null;
    public override IEnumerator Perform()
    {
        currentPerformable = null;
        IEnumerator current;
        while (IsComplete == false)
        {

            WanderPerformable wanderPerformable = new WanderPerformable(Performer);
            current = wanderPerformable.Perform();
            currentPerformable = wanderPerformable;
            var relevantResources = new List<IItemsProviderIntel<IResource>>();
            while (relevantResources.Count <= 0 && !IsComplete)
            {
                relevantResources.Clear();
                foreach (var code in resourcesToSearchFor)
                {
                    relevantResources.AddRange(Performer.GetResourcesInSight<IResource>(true).Where(intel => intel.Subject.HasItems));
                }
                relevantResources.Sort((x, y) => x.CompareTo(y));
                if (relevantResources.Count == 0)
                {
                    if (wanderPerformable.IsComplete || current == null || current.MoveNext() == false)
                    {
                        wanderPerformable = new WanderPerformable(Performer);
                        current = wanderPerformable.Perform();
                        currentPerformable = wanderPerformable;
                    }
                }
                else break;
                //need a look around  method, orelse creature will go to just wherever it happens to be looking

                yield return null;
            }

            if(relevantResources.Count > 0 && relevantResources[0] != null && !IsComplete)
            {
                AcquireItemPerformable harvestPerformable = relevantResources[0].Subject.GetInteractionPerformable(Performer.Body);
                currentPerformable = harvestPerformable;
                current = harvestPerformable?.Perform();
                while (!harvestPerformable.IsComplete && current != null && current.MoveNext()&& !this.IsComplete && resourcesToSearchFor.Any((t)=>t.IsAssignableFromAny(harvestPerformable.ItemSource.ItemTypes)))
                {
                    yield return null;
                }
            }

            yield return null;
        }
        _isComplete = true;
        currentPerformable = null;
        yield break;
        //yield return wander around, checking if there are any of the given resource in sight range after each yield return
        //if so, yield return moveToDestination the resource. When in range, query if there are enemies in the area, and decide fight or flight for the resource
        //then request harvestResource until inventory is full or danger approaches or resource is empty
    }
}