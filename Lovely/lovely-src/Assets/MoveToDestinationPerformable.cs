﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MoveToDestinationPerformable : Performable
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

    public override ActivityState ActivityType { get { return ActivityState.Nothing; } }
    Action<bool> callBack;
    Vector3 destination;
    bool acceptClosestLocation = false;
    float stoppingDistance = 1f;
    bool isUrgent = true;
    NavMeshAgent navAgent { get { return Performer.Body.NavAgent; } }

    public MoveToDestinationPerformable(PerceivingMind performer, Action<bool> callBack, Vector3 destination, bool acceptClosestLocation = false, float stoppingDistance = 1f, bool isUrgent = true) : base(performer)
    {
        base._performer = performer;
        this.callBack = callBack;
        this.destination = destination;
        this.acceptClosestLocation = acceptClosestLocation;
        this.stoppingDistance = stoppingDistance;
        this.isUrgent = isUrgent;
    }

    public override IEnumerator Perform()
    {
        var current = Performer.Body.MoveToDestination(destination, stoppingDistance);
        while(!IsComplete && current.MoveNext())
        {
            yield return null;
        }
        _isComplete = true;
        callBack(current.Current == ProgressStatus.Complete);
        yield break;
    }
}
