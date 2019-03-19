using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackPerformableDecisionMaker : IPerformable, IDecisionMaker
{
    Mind performer;
    Body Body { get { return performer.Body; } }

    public AttackPerformableDecisionMaker(Mind performer)
    {

    }

    public Mind Performer { get { throw new System.NotImplementedException(); } }

    public IPerformable GetDecisions()
    {
        throw new System.NotImplementedException();
    }

    public IEnumerator Perform()
    {
        //acquire enemy, giving preference to closer enemies, then glass cannons
        //choose attack ( if no attack, run to a safe distance, which is defined as 90% of powerSprintDistance) 
        //move (or sprint) to within 90% of the attack range( ie if blast has a range of 100m, move to 90m) to prevent whiffing
        //attack
        //repeat


        //acquire enemy, giving preference to closer enemies, then glass cannons
        var enemies = performer.VisibleEnemies;
        //enemies is sorted by distance, so enemies[0] is the closest
        if (enemies.Count <= 0) yield break;
        var current = enemies[0];
        //choose attack ( if no attack, run to a safe distance, which is defined as 90% of powerSprintDistance) 

        yield break;
    }
}
