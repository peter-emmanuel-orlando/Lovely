using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightPerformable : IPerformable
{
    Mind performer;

    public FightPerformable(Mind performer)
    {
        this.performer = performer;
    }

    public Mind Performer { get { return performer; } }

    public IEnumerator Perform()
    {
        var currentStrategy = GetStrategy();
        IEnumerator currentEnumerator = null;
        while (true)
        {
            var strategy = GetStrategy();
            if (currentEnumerator == null || strategy != currentStrategy)
            {
                currentStrategy = strategy;

                if (strategy == FightStrategy.Attack)
                    currentEnumerator = Attack();
                else if (strategy == FightStrategy.Flee)
                    currentEnumerator = Flee();
                else if (strategy == FightStrategy.Recoup)
                    currentEnumerator = Recoup();
                else if (strategy == FightStrategy.SafeDistance)
                    currentEnumerator = SafeDistance();
            }

            if (currentEnumerator != null && !currentEnumerator.MoveNext())
                currentEnumerator = null;

            if (strategy == FightStrategy.None)
                yield break;
            else
                yield return null;
        }
    }

    private IEnumerator Attack()
    {
        var enemy = GetBestAttackTarget();
        if(enemy.isInitialized)
        {
            var chase = new ChasePerformable(performer, enemy.subject);
            var chaseEnumerator = chase.Perform();
            while(chaseEnumerator.MoveNext())
            {
                yield return null;
            }
            var faceEnemyEnumerator = performer.Body.TurnToFace(enemy.subject.transform.position);
            while (chaseEnumerator.MoveNext())
            {
                yield return null;
            }
            //what if no attacks?
            var statusEnumerator = performer.AllAbilities[0].CastAbility();
            while(statusEnumerator.MoveNext())
            {
                yield return null;
            }
        }
        else
        {
            yield break;
        }
    }

    private IEnumerator Flee()
    {
        yield break;
    }

    private IEnumerator SafeDistance()
    {
        yield break;
    }

    private IEnumerator Recoup()
    {
        yield break;
    }

    private FightStrategy GetStrategy()
    {
        //based on the 3 closest enemies an 3 most dangerous enemies

        var result = FightStrategy.None;

        if (performer.VisibleEnemies.Count > 0)
            result = FightStrategy.Attack;

        return result;
    }

    private Intel GetBestAttackTarget()
    {
        var result = new Intel();
        if (performer.VisibleEnemies.Count > 0)
            result = performer.VisibleEnemies[0];
        return result;
    }

    private enum FightStrategy
    {
        None = 0,
        Attack,
        Flee,
        SafeDistance,
        Recoup,
    }



    /*///////////////////////////////////////////
    //fighting strategies!
    *////////////////////////////////////////////

    /*
     * based on attack combos. Specials are used ONLY for special reasons, those being the following:
     * it breaks an expected attack pattern
     * player has whiffed
     * it can break guard
     * to disrupt recouperaton
     * 
     * 
     * Bait/Feint
     * an attack pattern that begins like another attack pattern, but diverges at end
     * using an attack pattern trains the opponent to react to it. Setting the opponent up to 
     * rect to that pattern then changing the ending catches them by suprise
     * 
     * press the advantage
     * when the balance of defensive counters favors attacker, press the attack, ie magic gague is higher,
     * shield is stronger, stamina or replaces are higher
     * 
     * use their resources
     * getting into range to draw an attack then blocking, or getting in and out of range again can 
     * use up some opponents resources to get the advantage to then press
     * 
     * passive split:
     * when both characters are low on resources, attempt a retreat at earliest safe time in order to replenish reserves.
     * vulnerable during retreat
     * 
     * aggressive split
     * when both characters are low, press the attack so they must use thir remaining defensive resources first, 
     * then attack. attempt a knockdown and replenish during opponent recovery. Vulnerable to miscalculations
     * 
     * start with different moves every time to mix up enemy
     * 
     */
}
