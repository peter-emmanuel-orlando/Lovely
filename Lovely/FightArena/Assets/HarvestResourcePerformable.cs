using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class HarvestResourcePerformable : Performable
{

    //change to each mindStat in in-game days. negative takes away, positive adds
    public override float DeltaWakefulness { get { return _deltaWakefulness; } }//decreases when awake, depletes quicker when performing taxing tasks, increases 1.5x speed when sleeping
    public override float DeltaExcitement { get { return _deltaExcitement; } }//depletes awake or asleep. Tedious work takes chunks from this. relaxation activities increase this
    public override float DeltaSpirituality { get { return _deltaSpirituality; } }//depletes awake or asleep. relaxation activities increase this. Tedious Work or seedy activities takes chunks from this
    public override float DeltaSocialization { get { return _deltaSocialization; } }//depletes when awake. increases when working or playing together
    public override float DeltaCalories { get { return _deltaCalories; } }//num days calories will last.
    public override float DeltaBlood { get { return _deltaBlood; } }//reaching 0 blood and being passes out
    public override bool IsSleepActivity { get { return _isSleepActivity; } }//is this activity sleeping?

    float _deltaWakefulness = 1;
    float _deltaExcitement = 0;
    float _deltaSpirituality = 0;
    float _deltaSocialization = 0;
    float _deltaCalories = 1;
    float _deltaBlood = 0;
    bool _isSleepActivity = false;
    //*//////////////////////////////////////////////////////////////////////////////////////////////////

    public override ActivityState ActivityType { get { return ActivityState.Work; } }


    Resource _resourceToHarvest;
    public Resource resourceToHarvest { get { return _resourceToHarvest; } }
    protected abstract AnimationClip harvestAnimationClip { get; }

    //************************************************************************************************************************************
    //in the animator controller there is a placeholder animation named "Interact". This gets the override controller from Being and changes the 
    //AnimationClip named "Interact" to this AnimationClip. After this Interact is done, it returns the old clip
    bool hasOverriddenClip = false;
    AnimationClip overriddenAnimationClip;
    void AddAnimationToPerformer()
    {
        var controller = Performer.Body.overrideController;
        overriddenAnimationClip = controller["Interact"];
        if (overriddenAnimationClip == null)
            throw new UnityException("there must be a place holder animation named 'Interact' for this Interact to override");
        controller["Interact"] = harvestAnimationClip;
    }
    void ResetAnimationForPerformer()
    {
        if (hasOverriddenClip)
        {
            Performer.Body.overrideController[harvestAnimationClip.name] = overriddenAnimationClip;
            hasOverriddenClip = false;
        }
    }
    //************************************************************************************************************************************

    public HarvestResourcePerformable(Mind harvester, Resource resourceToHarvest)
    {
        base._performer = harvester;
        this._resourceToHarvest = resourceToHarvest;
    }

    public override IEnumerator Perform()
    {
        //move to resource
        //harvest resource
        var current = new MoveToDestinationPerformable(Performer, null, resourceToHarvest.transform.position).Perform();
        while (current.MoveNext())
            yield return null;
        if (resourceToHarvest != null && resourceToHarvest.hasResources && resourceToHarvest.CanBeingHarvest(Performer.Body))
        {
            /*
            AddAnimationToPerformer();
            if (harvestAnimationClip != null)
                Performer.Body.anim.SetTrigger("StartInteraction");
             */
        }
        IItem result;
        while (resourceToHarvest != null && resourceToHarvest.HarvestResource(Performer.Body, out result))
        {  
            //pick up item performable            
            yield return null;
        }
        _success = true;
        _isComplete = true;
        ResetAnimationForPerformer();
        yield break;
    }

    public override void Abort()
    {
        base.Abort();
        ResetAnimationForPerformer();
    }
}