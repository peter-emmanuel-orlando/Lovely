﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnifiedController;

public class AcquireItemPerformable : Performable
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
    public ItemsProvider ItemSource { get; }
    protected virtual AnimationClip AcquisitionAnimation { get; } = null;

    public AcquireItemPerformable(PerceivingMind acquisitioner, ItemsProvider itemSource, AnimationClip acquisitionAnimation = null) : base(acquisitioner)
    {
        base._performer = acquisitioner;
        this.ItemSource = itemSource;
        this.AcquisitionAnimation = acquisitionAnimation;
    }
    public override IEnumerator Perform()
    {
        //move to item
        //acquire item
        IEnumerator current = null;
        var maxTries = 5;
        for (int i = 0; i < maxTries; i++)
        {
            var success = false;
            current = new MoveToDestinationPerformable(Performer, (b)=>success = b, ItemSource.Bounds.ClosestPoint(Performer.Body.transform.position)).Perform();
            while (current.MoveNext())
                yield return null;

            if (success) break;
            else if (!success && i == maxTries - 1) yield break;
        }

        //deals with timing, either harvest time or animation
        #region 
        var harvestTime = ItemSource.harvestTime;
        var giveUpTime = Time.time + harvestTime ?? 10;
        if (AcquisitionAnimation != null && ItemSource != null && ItemSource.HasItems && ItemSource.CanBeAcquiredBy(Performer))
        {
            PlayToken pt = null;
            while (Time.time < giveUpTime)
            {
                pt = Performer.Body.PlayAnimation(AcquisitionAnimation, false, false, !harvestTime.HasValue );
                if (pt == null)
                    yield return null;
                else
                    break;
            }

            while ((harvestTime.HasValue && Time.time < giveUpTime) || (!harvestTime.HasValue && pt != null && pt.GetProgress() < 0.99))
            {
                yield return null;
            }
        }
        while (harvestTime.HasValue && Time.time < giveUpTime)
        {
            yield return null;
        }
        #endregion

        if (ItemSource != null && ItemSource.HasItems && ItemSource.Acquire(Performer.Body, out List<IItem> result, out List<ISpawnedItem<IItem>> spawnedItems))
        {
            //pick up item performable     
            foreach (var item in result)
            {
                Performer.Body.Backpack.AddItem(item);
            }
            foreach (var item in spawnedItems)
            {
                current = item.GetInteractionPerformable(Performer.Body).Perform();
                while (current.MoveNext())
                    yield return null;
            }
            yield return null;
        }
        _success = true;
        _isComplete = true;
        yield break;
    }
}