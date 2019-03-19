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
    ItemType resourcesToSearchFor;
    float SearchRadius { get { return Performer.SightRadius; } }

    public ForageForResourcesPerformable(Mind performer, ItemType resourcesToSearchFor) : base(performer)
    {
        base._performer = performer;
        this.resourcesToSearchFor = resourcesToSearchFor;
    }

    public ForageForResourcesPerformable(Mind performer) : base(performer)
    {
        base._performer = performer;
        this.resourcesToSearchFor = (ItemType)0xFF;
    }

    public void ChangeDesiredResources(ItemType relevantResources)
    {
        resourcesToSearchFor = relevantResources;
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
            var relevantResources = new List<ResourceIntel>();
            while (relevantResources.Count <= 0 && !IsComplete)
            {
                relevantResources.Clear();
                foreach (var code in resourcesToSearchFor.Enumerate())
                {
                    relevantResources.AddRange(Performer.GetResourcesInSight(code));
                }
                relevantResources.Sort();
                if(relevantResources.Count <= 0)
                {
                    if (wanderPerformable.IsComplete || current == null || current.MoveNext() == false)
                    {
                        wanderPerformable = new WanderPerformable(Performer);
                        current = wanderPerformable.Perform();
                        currentPerformable = wanderPerformable;
                    }
                }
                //need a look around  method, orelse creature will go to just wherever it happens to be looking
                //else break;

                yield return null;
            }

            if(relevantResources.Count > 0 && relevantResources[0] != null && !IsComplete)
            {
                HarvestResourcePerformable harvestPerformable = relevantResources[0].subject.GetHarvestPerformable(Performer.Body);
                current = harvestPerformable.Perform();
                currentPerformable = harvestPerformable;
                while (!harvestPerformable.IsComplete && current != null && current.MoveNext() == true && !IsComplete && resourcesToSearchFor.ContanisAny( harvestPerformable.resourceToHarvest.providedItemType))
                {
                    yield return null;
                }
            }
        }
        _isComplete = true;
        currentPerformable = null;
        yield break;
        //yield return wander around, checking if there are any of the given resource in sight range after each yield return
        //if so, yield return moveToDestination the resource. When in range, query if there are enemies in the area, and decide fight or flight for the resource
        //then request harvestResource until inventory is full or danger approaches or resource is empty
    }
}