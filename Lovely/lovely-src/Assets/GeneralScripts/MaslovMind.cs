using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SurvivalMind : PerceivingMind
{
    public SurvivalMind(Body body) : base(body) { }
    IPerformable decision = null;
    public override IPerformable GetDecisions()
    {
        if (decision != null && decision.IsComplete) decision = null;
        var newDecision = decision;
        Decide(ref newDecision);//NOT this.Decide decide may be overridden
        //if decision is empty, decided to do nothing, or not the same as current decision; decision = newDecision
        if (decision == null || newDecision == null || decision.GetType() != newDecision.GetType())
            decision = newDecision;
        return decision;
    }
    protected virtual bool Decide(ref IPerformable newDecision)
    {
        var hasDecided = false;
        //decide survival decisions
        return hasDecided;
    }
}
public abstract class SafetyMind : SurvivalMind
{
    public SafetyMind(Body body) : base(body) { }
    protected override bool Decide(ref IPerformable newDecision)
    {
        var hasDecided = base.Decide(ref newDecision);
        if (!hasDecided)
        {
            //decide safety decisions
        }
        return hasDecided;
    }
}
public abstract class EsteemMind : SafetyMind
{
    public LivingPlace Home { get; protected set; }// if no home, create a temporary one
    public WorkPlace WorkPlace { get; protected set; }//this is by default food storage. you work to fill up your food storage, by hunting and preserving//by default workplace is just the urge to hunt
    public List<RecreationPlace> RestPlaces { get; protected set; } = new List<RecreationPlace>();//by default interaction with peers or enjoying a local natural feature


    public EsteemMind(Body body) : base(body) { }


    protected override bool Decide(ref IPerformable newDecision)
    {
        //if no backpack, hunt for materials to make a backpack or buy one.
        //If no job, wander around and then choose among the least populated jobs, or request one from buildings
        //if no house, go build one, or buy one, or establish a temporary lean-to or hotel
        var hasMadeDecision = base.Decide(ref newDecision);

        //"emergency" meaning imminant danger, respond to imminant danger regardless of currentState
        //emergency is...
        //hunger is above a certain level
        //cold is above a certain
        //bleeding
        //in combat
        //sleep is above level
        //start emergency conditions

        /*
        if (!hasMadeDecision && Body.Blood < 0.3 * Body.BloodMax)
            hasMadeDecision = new FightOrFlightPerformable();
        if (!hasMadeDecision && Body.Calories < 0.6 * Body.CaloriesMax)
            hasMadeDecision = new SearchForFoodPerformable();
        */
        //end emergency conditions

        if (!hasMadeDecision && IsWorkPeriod)
            hasMadeDecision = GetWorkPeriodDecisions(ref newDecision);

        if (!hasMadeDecision && IsSleepPeriod)
            hasMadeDecision = GetSleepPeriodDecisions(ref newDecision);

        if (!hasMadeDecision && IsRecreationPeriod)
            hasMadeDecision = GetRecreationPeriodDecisions(ref newDecision);

        if (!hasMadeDecision)
            hasMadeDecision = GetMaintinenceDecisions(ref newDecision);

        if (!hasMadeDecision)
        {
            //if you make no decisions, you've deccided to nothing (idle)
            //realistically though, youd be preening or sleeping or sitting/meditating or wandering
            hasMadeDecision = true;
            newDecision = new AbortPerformable();
        }


        return hasMadeDecision;
    }
    //perhaps these are defined by inheritor?
    //ie maintainHome, maintainPersonalStorage etc
    bool GetMaintinenceDecisions(ref IPerformable newDecision)
    {

        var hasMadeDecision = false;

        //..get performable from ItemPack
        //      clean and sharpen weapons
        //      refill foodPack
        //      etc
        if (!hasMadeDecision && base.Backpack != null)
            hasMadeDecision = base.Backpack.GetMaintinenceAssignment(Body, ref newDecision);



        //..get performable from home
        //      find a home if there is none
        //      clean/maintain home
        //      patrol /maintain home perimeter
        //      hunt to fill home food reserves
        if (!hasMadeDecision && Home != null)
            hasMadeDecision = Home.GetMaintinenceAssignment(Body, ref newDecision);



        //..get performable from body
        //      preen
        //      sharpen talons
        //      groom
        //      eat
        //      poo
        if (!hasMadeDecision)
            hasMadeDecision = Body.GetMaintinenceAssignment(Body, ref newDecision);

        return hasMadeDecision;
    }

    bool GetWorkPeriodDecisions(ref IPerformable newDecision)
    {
        //do wakeupfromBedPerformable before working

        // try to find work, if not try to find a relaxation activity, or sleep
        var hasMadeDecision = false;

        //work performable. work is a communal idea. Work is something you do for the community that indirectly benefits you
        //get work performable.
        if (WorkPlace != null)
            hasMadeDecision = WorkPlace.GetAssignment(Body, ref newDecision);

        //if no work, do chores
        if (!hasMadeDecision)
            hasMadeDecision = GetMaintinenceDecisions(ref newDecision);

        //if NONE of this, treat this as a recreationPeriod
        if (!hasMadeDecision)
            GetRecreationPeriodDecisions(ref newDecision);

        return hasMadeDecision;
    }

    bool GetRecreationPeriodDecisions(ref IPerformable newDecision)
    {
        // TODO: implement this
        var hasMadeDecision = false;
        return hasMadeDecision;
    }



    bool GetSleepPeriodDecisions(ref IPerformable newDecision)
    {
        var hasMadeDecision = false;

        //if already sleeping, stay asleep (aside from a random chance of waking)
        if (newDecision != null && newDecision.ActivityType == ActivityState.Rest)
        {
            hasMadeDecision = true;
        }
        else
        {
            //give sleep priority, but decide to nightlife, or relax

            //if not that sleepy but kinda bored, treat this as a recreationPeriod
            if (Wakefulness > WakefulnessMax * 0.75f && Excitement < ExcitementMax * 0.75f)
                hasMadeDecision = GetRecreationPeriodDecisions(ref newDecision);

            //if not sleepy but not bored, do some maintinence before bed
            if (!hasMadeDecision && Wakefulness > WakefulnessMax * 0.9f)
                hasMadeDecision = GetMaintinenceDecisions(ref newDecision);

            //if no desire to do anythin else, finish up or abort whatever task then sleep
            if (!hasMadeDecision)
            {
                //do prepareForBedPerformable before sleeping
                if (newDecision == null || !newDecision.IsSleepActivity)
                    hasMadeDecision = Body.GetRestAssignment(Body, ref newDecision);
            }
        }

        return hasMadeDecision;
    }

}
public abstract class ActualizationMind : EsteemMind
{
    public ActualizationMind(Body body) : base(body) { }
    protected override bool Decide(ref IPerformable newDecision)
    {
        var hasDecided = base.Decide(ref newDecision);
        if (!hasDecided)
        {
            //decide actualization decisions
        }
        return hasDecided;
    }
}





//calories is magic. the ideal calorie balance gives a perfect balance between 
//physical stats and magic reserves. this body type is the classic fighter
//hoarding excessive calories results in a "warlock" who isnt nearly as mobile, but has hella magic up his sleeve
// extreme calories causes magic to bleed out of the skin as the body tries to purge the excess. these burn calories just by existing
//they are the typical "blaster mage
//low magic are the monks and the assassins
//starving creatures are antimages, they vampiricly pull magic from creatures
/*
public class AnimalBehaviorChoreographer : IDecisionMaker
{
//personality traits that decision making is based on is here

float magic = 0;
float intelligence = 0;
float agressiveness = 0;
float pursuasiveness = 0;
float perceptiveness = 0;
float friendlieness = 0;
// stats from body

/*
 * 1) fill your belly
 * 2) quench your thirst
 * 3) get clothes
 * 4) find temporary shelter
 * 5) get fire
 * 6) fill food storage
 * 7) make nest
 * 8) form hunting parties - follow herds of food
 * 9) form foraging parties
 * 
 * 
 * 
 * the concept of "mine"
 * a list of resources that are considtered "Mine" or ""ours"
 * a non authorized utilizer of a resource consitered "ours" prompts first warning, then an attack
 * 
 * if a territory or structure is consitered "mine" determining an intruder is more nuanced. 
 * Anyone not a resident or visitor is consitered an intruder in a space marked "private". this reflects that you should know of anyone coming or visiting your house
 * anyone who isnt a resident in a space marked ""
 * 
 * anyone being entering or residing in a place must request an AdmissionToken from another Being
 * An AdmissionToken says the issuer, the recipiant, the building its in regards to, the level of access, InvalidatingConditions,  and IssuingRights; the right to give lower tier access to others
 * The AdmissionToken checks every frame if there are any conditions that would Invalidate it
 * Intruders can request a ForgedAdmissionToken that is just an admission token that is default invalid and has a authenticity rating
 * The method public bool GetIsValid(Being checker) checks if anyone is here that doesnt belong here.
 * The checkers Perceptiveness minus the Token holders (pursuasiveness - suspiciosness) and the checkers own AdmissionToken determine whether 
 * a checker can detect a invalid AdmissionToken
 * A Place should have the method GetSuspiciousnessLevel(Being possibleIntruder) that outputs a suspiciosness level based on whats visible of the being and what should be the norms in that building
 * 
 * In public areas, the AdmissionToken is issued based soley on visual aspects such as features, race, dress etc. Failing to have a valid AdmissionToken in 
 * public means other Beings will avoid you. Police will determine seperately wheather to attack or not
 * 
 * an "Outcry" has a "Volume" this determines the radius in which police will be summoned
 */
