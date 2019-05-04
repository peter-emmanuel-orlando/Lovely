using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackPerformableDecisionMaker : Performable, IDecisionSource
{
    //change to each mindStat in in-game days. negative takes away, positive adds
    public override float DeltaWakefulness { get { return _deltaWakefulness; } }//decreases when awake, depletes quicker when performing taxing tasks, increases 1.5x speed when sleeping
    public override float DeltaExcitement { get { return _deltaExcitement; } }//depletes awake or asleep. Tedious work takes chunks from this. relaxation activities increase this
    public override float DeltaSpirituality { get { return _deltaSpirituality; } }//depletes awake or asleep. relaxation activities increase this. Tedious Work or seedy activities takes chunks from this
    public override float DeltaSocialization { get { return _deltaSocialization; } }//depletes when awake. increases when working or playing together
    public override float DeltaCalories { get { return _deltaCalories; } }//num days calories will last.
    public override float DeltaBlood { get { return _deltaBlood; } }//reaching 0 blood and being passes out
    public override bool IsSleepActivity { get { return _isSleepActivity; } }//is this activity sleeping?

    float _deltaWakefulness = 0;
    float _deltaExcitement = 0;
    float _deltaSpirituality = 0;
    float _deltaSocialization = 0;
    float _deltaCalories = 0;
    float _deltaBlood = 0;
    bool _isSleepActivity = false;
    //*//////////////////////////////////////////////////////////////////////////////////////////////////

    public override ActivityState ActivityType { get { return ActivityState.Nothing; } }
    
    public AttackPerformableDecisionMaker(PerceivingMind performer) : base(performer)
    {

    }

    public IPerformable GetDecisions()
    {
        throw new System.NotImplementedException();
    }

    public override IEnumerator Perform()
    {
        //acquire enemy, giving preference to closer enemies, then glass cannons
        //choose attack ( if no attack, run to a safe distance, which is defined as 90% of powerSprintDistance) 
        //move (or sprint) to within 90% of the attack range( ie if blast has a range of 100m, move to 90m) to prevent whiffing
        //attack
        //repeat


        //acquire enemy, giving preference to closer enemies, then glass cannons
        var enemies = Performer.VisibleEnemies;
        //enemies is sorted by distance, so enemies[0] is the closest
        if (enemies.Count <= 0) yield break;
        var current = enemies[0];
        //choose attack ( if no attack, run to a safe distance, which is defined as 90% of powerSprintDistance) 

        yield break;
    }
}
