using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightPerformable : IPerformable
{
    Mind performer;
    public Mind Performer { get { return performer; } }

    public IEnumerator Perform()
    {
        yield break;
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
