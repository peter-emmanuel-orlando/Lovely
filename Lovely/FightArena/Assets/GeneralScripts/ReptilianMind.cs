using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class ReptilianMind : Mind
{
    public ReptilianMind(Body body) : base(body)
    {
    }
}
    /*
    public LivingPlace home;// if no home, create a temporary one
    public WorkPlace workPlace;//this is by default food storage. you work to fill up your food storage, by hunting and preserving//by default workplace is just the urge to hunt
    public ItemPack backpack;
    public List<RecreationPlace> restPlaces = new List<RecreationPlace>();//by default interaction with peers or enjoying a local natural feature

    //calories is magic. the ideal calorie balance gives a perfect balance between 
    //physical stats and magic reserves. this bogy type is the classic fighter
    //hoarding excessive calories results in a "warlock" who isnt nearly as mobile, but has hella magic up his sleeve
    // extreme calories causes magic to bleed out of the skin as the body tries to purge the excess. these burn calories just by existing
    //they are the typical "blaster mage
    //low magic are the monks and the assassins
    //starving creatures are antimages, they vampiricly pull magic from creatures

    //number of in-game days of each stat Mind has left
    float wakefulness { get { return being.mind.wakefulness; } }//decreases when awake, depletes quicker when performing taxing tasks, increases 1.5x speed when sleeping
    float excitement { get { return being.mind.excitement; } }//depletes awake or asleep. Tedious work takes chunks from this. relaxation activities increase this
    float spirituality { get { return being.mind.spirituality; } }//depletes awake or asleep. relaxation activities increase this. Tedious Work or seedy activities takes chunks from this
    float socialization { get { return being.mind.socialization; } }//depletes when awake. increases when working or playing together
    float calories { get { return being.mind.calories; } }//num days calories will last.
    float blood { get { return being.mind.blood; } }//reaching 0 blood and being passes out
    bool isAwake { get { return being.mind.isAwake; } }

    //max number of in-game days of each stat Mind can hold
    float wakefulnessMax { get { return being.mind.wakefulnessMax; } }
    float excitementMax { get { return being.mind.excitementMax; } }
    float spiritualityMax { get { return being.mind.spiritualityMax; } }
    float socializationMax { get { return being.mind.socializationMax; } }
    float caloriesMax { get { return being.mind.caloriesMax; } }
    float bloodMax { get { return being.mind.bloodMax; } }


    bool IsWorkPeriod { get { return being.mind.IsWorkPeriod; } }
    bool IsRecreationPeriod { get { return being.mind.IsRecreationPeriod; } }
    bool IsSleepPeriod { get { return being.mind.IsSleepPeriod; } }

    protected override float SightRange
    {
        get
        {
            throw new System.NotImplementedException();
        }
    }

    IPerformable decision = null;


    

    protected override void  Update()
    {
        base.Update();

        if (home == null) home = UnityEngine.GameObject.FindObjectOfType<LivingPlace>();
        if (decision != null && decision.isComplete) decision = null;

        Body.gameObject.DisplayTextComponent(this);
    }
    

    public ReptilianMind(Body body): base(body)
    {

    }
    

    public override string ToString()
    {
        string s =
            "wakefulness: " + wakefulness + "\n" +
            "wakefulnessMax: " + wakefulnessMax + "\n" +
            "--------------------------------------------\n" +
            "excitement: " + excitement + "\n" +
            "excitementMax: " + excitementMax + "\n" +
            "--------------------------------------------\n" +
            "spirituality: " + spirituality + "\n" +
            "spiritualityMax: " + spiritualityMax + "\n" +
            "--------------------------------------------\n" +
            "socialization: " + socialization + "\n" +
            "socializationMax: " + socializationMax + "\n" +
            "--------------------------------------------\n" +
            "calories: " + calories + "\n" +
            "caloriesMax: " + caloriesMax + "\n" +
            "--------------------------------------------\n" +
            "blood: " + blood + "\n" +
            "bloodMax: " + bloodMax + "\n" +
            "--------------------------------------------\n" +
            "\n" +
            "isAwake: " + isAwake + "\n" +
            "IsWorkPeriod: " + IsWorkPeriod + "\n" +
            "IsRecreationPeriod: " + IsRecreationPeriod + "\n" +
            "IsSleepPeriod: " + IsSleepPeriod + "\n" +
            "--------------------------------------------\n" +
            "\n" +
            "ActivityState: " + currentState + "\n" +
            "currentPerformable: " + being.mind.currentPerformable + "\n" +
            "decision: " + decision + "\n"
        ;
        return s;
    }

    public override IPerformable GetDecisions()
    {
        if (decision != null && decision.isComplete) decision = null;
        var newDecision = decision;
        var hasMadeDecision = false;

        //"emergency" meaning imminant danger, respond to imminant danger regardless of currentState
        //emergency is...
        //hunger is above a certain level
        //cold is above a certain
        //bleeding
        //in combat
        //sleep is above level
        //start emergency conditions

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

        //if decision is not the same as current decision, decision = newDecision
        if (decision == null || newDecision == null || newDecision.GetType() != decision.GetType())
            decision = newDecision;

        return decision;
    }
    //perhaps these are defined by inheritor?
    //ie maintainHome, maintainPersonalStorage etc
    bool GetMaintinenceDecisions(ref IPerformable newDecision)
    {
        //if no backpack, hunt for materials to make a backpack or buy one.
        //If no job, wander around and then choose among the least populated jobs, or request one from buildings
        //if no house, go build one, or buy one, or establish a temporary lean-to or hotel

        var hasMadeDecision = false;

        //..get performable from ItemPack
        //      clean and sharpen weapons
        //      refill foodPack
        //      etc
        if (!hasMadeDecision && backpack != null)
            hasMadeDecision = backpack.GetMaintinenceAssignment(being, ref newDecision);



        //..get performable from home
        //      find a home if there is none
        //      clean/maintain home
        //      patrol /maintain home perimeter
        //      hunt to fill home food reserves
        if (!hasMadeDecision && home != null)
            hasMadeDecision = home.GetMaintinenceAssignment(being, ref newDecision);



        //..get performable from body
        //      preen
        //      sharpen talons
        //      groom
        //      eat
        //      poo
        if (!hasMadeDecision)
            hasMadeDecision = being.body.GetMaintinenceAssignment(being, ref newDecision);

        return hasMadeDecision;
    }

    bool GetWorkPeriodDecisions(ref IPerformable newDecision)
    {
        //do wakeupfromBedPerformable before working

        // try to find work, if not try to find a relaxation activity, or sleep
        var hasMadeDecision = false;

        //work performable. work is a communal idea. Work is something you do for the community that indirectly benefits you
        //get work performable.
        if (workPlace != null)
            hasMadeDecision = workPlace.GetAssignment(being, ref newDecision);

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
        var hasMadeDecision = false;
        return hasMadeDecision;
    }



    bool GetSleepPeriodDecisions(ref IPerformable newDecision)
    {
        var hasMadeDecision = false;

        //if already sleeping, stay asleep (aside from a random chance of waking)
        if (decision != null && decision.isSleepActivity)
        {
            hasMadeDecision = true;
            newDecision = decision;
        }
        else
        {
            //give sleep priority, but decide to nightlife, or relax

            //if not that sleepy but kinda bored, treat this as a recreationPeriod
            if (wakefulness > wakefulnessMax * 0.75f && excitement < excitementMax * 0.75f)
                hasMadeDecision = GetRecreationPeriodDecisions(ref newDecision);

            //if not sleepy but not bored, do some maintinence before bed
            if (!hasMadeDecision && wakefulness > wakefulnessMax * 0.9f)
                hasMadeDecision = GetMaintinenceDecisions(ref newDecision);

            //if no desire to do anythin else, finish up or abort whatever task then sleep
            if (!hasMadeDecision)
            {
                hasMadeDecision = true;
                //do prepareForBedPerformable before sleeping
                if (newDecision == null || !newDecision.isSleepActivity)
                    newDecision = new SleepPerformable(being, delegate () { return decision == null || !decision.isSleepActivity; });
            }
        }

        return hasMadeDecision;
    }
    
}

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
     *


    Being being;


    public AnimalBehaviorChoreographer (Being being, UpdateRegistrar updateRegistrationMethod)
    {
        this.being = being;
        updateRegistrationMethod(Update);
    }


    // there is an invisible sleep energy. Sleeping replenishes this energy bar. Sleeping during designated sleeping hours gives a bonus to the replenishment rate

    float sleepPeriodStart = 0;//time of sleep period start
    float sleepPeriodEnd= 5;//length of sleep period
    bool isSleepTime
    {
        get
        {
            var result = false;
            if (sleepPeriodStart > sleepPeriodEnd)
            {
                if (GameTime.Hour > sleepPeriodStart || GameTime.Hour < sleepPeriodEnd) result = true;
            }
            if (sleepPeriodStart < sleepPeriodEnd)
            {
                if (GameTime.Hour > sleepPeriodStart && GameTime.Hour < sleepPeriodEnd) result = true;
            }
            return result;
        }
    }

    float restRecoveryRate = 3.5f;//each hour recovers 3.5 hours of sleep, netting 2.5
    float restRecoveryBonusRate = 5f;//each hour recovers 5 hours of sleep, netting 4

    float restLevel = restLevelMax;// counts down from max to zero. overall disadvantage under half peaking at .  intermittant blackouts, greyouts and halucinations below 
    const float restLevelMax = 15f;//days of rest that can be stored

    float GetSleepPriority()
    {
        //by default, sleep priority = .3 during sleep hours, 0 during wake hours
        var result = 0f;
        if (isSleepTime) result += 0.3f;

        result +=  (restLevelMax - restLevel)/restLevelMax;
        result = Mathf.Clamp(result, 0f, 0.8f);
        return result;
    }

    float GetEatPriority()
    {
        return 0f;
    }
    float GetMatingPriority()
    {
        return 0f;
    }


    float GetFightPriority()
    {
        return 0f;
    }
    float GetFlightPriority()
    {
        return 0f;
    }
    
    void Update()
    {
        if(isSleepTime)
            restLevel -= GameTime.DeltaTimeGameDays;
    }

    public IPerformable GetDecisions()
    {
        var maxPriority = 0;
        var decision = new EmptyPerformable();

        if(GetSleepPriority() > maxPriority)
        {

        }
        return decision;
    }
}
*/
