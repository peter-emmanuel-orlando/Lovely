using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//fight performable should be called MartialDecicionMaker which is an IDecisionMaker that returns either attack, or flee or recoup or whatever[each of which should be a performable]
public class FightPerformable : Performable
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

    private PerceivingMind performer;
    private readonly float reassessmentInterval = 2f;
    private float nextReassessment = 0;

    public FightPerformable(PerceivingMind performer)
    {
        this.performer = performer;
    }
    

    public override IEnumerator Perform()
    {
        var currentStrategy = GetStrategy();
        IEnumerator currentEnumerator = null;
        while (true)
        {
            var strategy = GetStrategy();
            if (currentEnumerator == null || strategy != currentStrategy || Time.time >= nextReassessment )
            {
                nextReassessment = Time.time + reassessmentInterval;
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
        if(enemy != null)
        {
            IEnumerator activeEnumerator = null;
            
            var chase = new ChasePerformable(performer, enemy.subject);
            activeEnumerator = chase.Perform();
            while(activeEnumerator.MoveNext())
            {
                yield return null;
            }
            activeEnumerator = null;

            if(!enemy.subject.IsNull())
                activeEnumerator = performer.Body.TurnToFace(enemy.subject.Transform.position);
            while (activeEnumerator != null && activeEnumerator.MoveNext())
            {
                yield return null;
            }
            activeEnumerator = null;

            //what if no attacks?
            var chosenAbility = performer.Body.CharacterAbilities[CharacterAbilitySlot.DashPunch];
            chosenAbility.CastAbility();           
            while (chosenAbility != null && chosenAbility.CheckStatus() == ProgressStatus.InProgress)
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

    private BodyIntel GetBestAttackTarget()
    {
        BodyIntel result = null;
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
