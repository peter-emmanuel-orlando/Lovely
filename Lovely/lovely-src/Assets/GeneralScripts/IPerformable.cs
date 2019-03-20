using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPerformable
{
    Mind Performer { get; }
    ActivityState ActivityType { get; }

    //"yield return Tick();"is called every frame until this evaluates to true. If on the first call this is false, tick will never be called, but start and end will be called sequentially
    bool IsComplete { get; }
    bool IsPerforming { get; }

    IEnumerator Perform();

    //body looks at this when performable is complete to determine whether to call onSuccess or Onfaliure
    bool Success { get; }

    //should end everything.
    void Abort();


    //change to each mindStat in in-game days. negative takes away, positive adds
    float DeltaWakefulness { get; }//decreases when awake, depletes quicker when performing taxing tasks, increases 1.5x speed when sleeping
    float DeltaExcitement { get; }//depletes awake or asleep. Tedious work takes chunks from this. relaxation activities increase this
    float DeltaSpirituality { get; }//depletes awake or asleep. relaxation activities increase this. Tedious Work or seedy activities takes chunks from this
    float DeltaSocialization { get; }//depletes when awake. increases when working or playing together
    float DeltaCalories { get; }//num days calories will last.
    float DeltaBlood { get; }//reaching 0 blood and being passes out
    bool IsSleepActivity { get; }//is this activity sleeping?

    //tells if two performables are essentially the same
    //bool isEquivilantTo(IPerformable other);
}

