using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;




//responsible for...
//  perceiving the world around. this means quering all the spawnables in sight range and classifying them as friend/enemy/resource/danger etc.
public abstract class PerceivingMind : DecisionManager
{

    public PerceivingMind(Body body) : base(body)
    {

    }

    protected override void ReceiveUpdates(object senter, EventArgs e)
    {
        LookAround();
        UpdateMindStats();
        //I want mind to look around before it makes decisions, so this is after
        base.ReceiveUpdates(senter, e);
        Body.gameObject.DisplayTextComponent(this);
    }


    //====================================================================================================================================================================================
    //========================================================Perceiving and categorizing the world around================================================================================
    //====================================================================================================================================================================================
 
    #region [[[ perceiving and categorizing world ]]]
    readonly List<BodyIntel> visibleEnemies = new List<BodyIntel>();
    readonly List<BodyIntel> visibleAllies = new List<BodyIntel>();
    readonly List<BodyIntel> allBodiesInSightRange = new List<BodyIntel>();
    readonly Dictionary<ItemType, List<ResourceIntel>> allResourcesInSightRange = Item.GetItemTypeDictionary<List<ResourceIntel>>();
    
    public List<BodyIntel> VisibleEnemies { get { return new List<BodyIntel>(visibleEnemies); } }//change so they return an array
    public List<BodyIntel> VisibleAllies { get { return new List<BodyIntel>(visibleAllies); } }
    public List<BodyIntel> AllBeingsInSightRange { get { return new List<BodyIntel>(allBodiesInSightRange); } }
    public ResourceIntel[] GetResourcesInSight(ItemType type) { return allResourcesInSightRange[type].ToArray(); }

    public abstract float SightRadius { get; }//move to body
    //public abstract float SightArc { get; }//move to body

    private void LookAround()
    {
        visibleEnemies.Clear();
        var inRangeBodies = TrackedComponent<Body>.GetOverlapping(Body.CameraBone.transform.position, SightRadius);
        foreach (var current in inRangeBodies)
        {
            if (!ReferenceEquals(current, Body))
            {
                var intel = new BodyIntel(Body, current);
                allBodiesInSightRange.Add(intel);
                if (intel.relationship.FriendshipLevel <= 0)
                    visibleEnemies.Add(intel);
                else
                    visibleAllies.Add(intel);
            }
        }
        visibleEnemies.Distinct();
        visibleEnemies.Sort();
    }

    //determines if another being is a friend, enemy, or whatever.
    public virtual Relationship GetRelationship(PerceivingMind other)
    {
        return new Relationship(0, 0, false, false);
    }

    public virtual bool IsVisible(RelativePositionInfo other)
    {
        return (other.distance <= SightRadius);// && other.angle < SightArc) || (other.distance < 5f);
    }

    #endregion

    //====================================================================================================================================================================================
    //========================================================STATS FOR MIND==============================================================================================================
    //====================================================================================================================================================================================

    #region [[[ mind stats ]]]
    //bool isAwake = true;//doesnt work well. other performables may not set properly
    //awake condition. sleeping and awakening should happen internally
    //public putToSleep(func awake conditions)
    //public Awaken()


    //number of in-game days of each stat Mind has left
    public float Wakefulness { get; protected set; } = float.MaxValue;
    public float Excitement { get; protected set; } = float.MaxValue;
    public float Spirituality { get; protected set; } = float.MaxValue;
    public float Socialization { get; protected set; } = float.MaxValue;
    public bool IsAwake { get { if (CurrentPerformable == null) return true; else return !CurrentPerformable.IsSleepActivity; } }

    //max number of in-game days of each stat Mind can hold
    public float WakefulnessMax { get; protected set; } = float.MaxValue;
    public float ExcitementMax { get; protected set; } = float.MaxValue;
    public float SpiritualityMax { get; protected set; } = float.MaxValue;
    public float SocializationMax { get; protected set; } = float.MaxValue;
    #endregion

    #region [[[ from body, here for convenience ]]]
    public float Calories { get { return Body.Calories; } }
    public float CaloriesMax { get { return Body.CaloriesMax; } }
    public float Blood { get { return Body.Blood; } }
    public float BloodMax { get { return Body.BloodMax; } }
    public float Stamina { get { return Body.Stamina; } }
    public float StaminaMax { get { return Body.CaloriesMax; } }

    public ItemPack Backpack { get { return Body.Backpack; } }
    #endregion

    #region [[[ active period stats ]]]

    public void SetActivePeriods(float activePeriodStart, float workPeriodLength, float recreationPeriodLength, bool isNormalizedLength)
    {
        if (isNormalizedLength)
        {
            workPeriodLength *= GameTime.DaysToHours;
            recreationPeriodLength *= GameTime.DaysToHours;
        }
        this.ActivePeriodStart = Mathf.Clamp(activePeriodStart, 0f, GameTime.DaysToHours) % GameTime.DaysToHours;
        this.WorkPeriodLength = Mathf.Clamp(workPeriodLength, 0f, GameTime.DaysToHours);
        this.RecreationPeriodLength = Mathf.Clamp(recreationPeriodLength, 0f, GameTime.DaysToHours - workPeriodLength);
    }

    protected virtual float DefaultActivePeriodStart { get { return 0; } }
    protected virtual float DefaultWorkPeriodLengthNormalized { get { return 0.2f; } }
    protected virtual float DefaultrecreationPeriodLengthNormalized { get { return 0.5f; } }

    //active period, recreation period, rest period, emergency period
    public float ActivePeriodStart { get; private set; }//time of active period, and work start
= 0;//time of active period, and work start

    public float WorkPeriodStart { get { return ActivePeriodStart; } }//same as active period start
    public float WorkPeriodLength { get; private set; }//how long the work period lasts
= 0.25f * GameTime.DaysToHours;//how long the work period lasts
    public float WorkPeriodEnd { get { return (WorkPeriodStart + WorkPeriodLength) % GameTime.DaysToHours; } }//time of work period start

    public float RecreationPeriodStart { get { return WorkPeriodEnd; } }//same as work period end
    public float RecreationPeriodLength { get; private set; }//how long the recreation period lasts
= 0.5f * GameTime.DaysToHours;//how long the recreation period lasts
    public float RecreationPeriodEnd { get { return (RecreationPeriodStart + RecreationPeriodLength) % GameTime.DaysToHours; } }//time of recreation period end

    public float SleepPeriodStart { get { return RecreationPeriodEnd; } }//same as recreation period end
    public float SleepPeriodLength { get { return Mathf.Clamp(GameTime.DaysToHours - WorkPeriodLength - RecreationPeriodLength, 0f, float.MaxValue); } }//how long the sleepationn period lasts
    public float SleepPeriodEnd { get { return (SleepPeriodStart + SleepPeriodLength) % GameTime.DaysToHours; } }//time of sleep period end

    public bool IsWorkPeriod { get { return GameTime.IsBetweenHours(GameTime.Hour, WorkPeriodStart, WorkPeriodEnd); } }
    public bool IsRecreationPeriod { get { return GameTime.IsBetweenHours(GameTime.Hour, RecreationPeriodStart, RecreationPeriodEnd); } }
    public bool IsSleepPeriod { get { return GameTime.IsBetweenHours(GameTime.Hour, SleepPeriodStart, SleepPeriodEnd); } }
    public bool IsInEmergency { get { return false; } }
    #endregion


    void UpdateMindStats()
    {
        //decrement all stats because of time
        Wakefulness -= GameTime.DeltaTimeGameDays;
        Excitement -= GameTime.DeltaTimeGameDays;
        Spirituality -= GameTime.DeltaTimeGameDays;
        Socialization -= GameTime.DeltaTimeGameDays;
        
        //decrease stats based on performable
        if (CurrentPerformable != null)
        {
            Wakefulness += CurrentPerformable.DeltaWakefulness * GameTime.DeltaTimeGameDays * ((CurrentState == ActivityState.Rest && IsSleepPeriod) ? 2f : 1f);//decreases when awake, depletes quicker when performing taxing tasks, increases 1.5x speed when sleeping
            Excitement += CurrentPerformable.DeltaExcitement * GameTime.DeltaTimeGameDays;//depletes awake or asleep. Tedious work takes chunks from this. recreation activities increase this
            Spirituality += CurrentPerformable.DeltaSpirituality * GameTime.DeltaTimeGameDays;//depletes awake or asleep. recreation activities increase this. Tedious Work or seedy activities takes chunks from this
            Socialization += CurrentPerformable.DeltaSocialization * GameTime.DeltaTimeGameDays;//depletes when awake. increases when working or playing together
        }

        //clamp resulting stats
        Wakefulness = Mathf.Clamp(Wakefulness, 0, WakefulnessMax);
        Excitement = Mathf.Clamp(Excitement, 0, ExcitementMax);
        Spirituality = Mathf.Clamp(Spirituality, 0, SpiritualityMax);
        Socialization = Mathf.Clamp(Socialization, 0, SocializationMax);
    }
    //++++++++++++++++=====================================================


    public override string ToString()
    {
        string s =
            "wakefulness: " + Wakefulness + "\n" +
            "wakefulnessMax: " + WakefulnessMax + "\n" +
            "--------------------------------------------\n" +
            "excitement: " + Excitement + "\n" +
            "excitementMax: " + ExcitementMax + "\n" +
            "--------------------------------------------\n" +
            "spirituality: " + Spirituality + "\n" +
            "spiritualityMax: " + SpiritualityMax + "\n" +
            "--------------------------------------------\n" +
            "socialization: " + Socialization + "\n" +
            "socializationMax: " + SocializationMax + "\n" +
            "--------------------------------------------\n" +
            "calories: " + Calories + "\n" +
            "caloriesMax: " + CaloriesMax + "\n" +
            "--------------------------------------------\n" +
            "blood: " + Blood + "\n" +
            "bloodMax: " + BloodMax + "\n" +
            "--------------------------------------------\n" +
            "\n" +
            "isAwake: " + IsAwake + "\n" +
            "IsWorkPeriod: " + IsWorkPeriod + "\n" +
            "IsRecreationPeriod: " + IsRecreationPeriod + "\n" +
            "IsSleepPeriod: " + IsSleepPeriod + "\n" +
            "--------------------------------------------\n" +
            "\n" +
            "ActivityState: " + CurrentState + "\n" +
            "currentPerformable: " + CurrentPerformable + "\n" +
            "decisionMaker: " + DecisionSource + "\n"
        ;
        return s;
    }
}




/*
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Mind : IDecisionMaker
{

    //properties
    private Body body;
    protected ActivityState currentState;
    private IDecisionMaker decisionSource;
    //private Dictionary<IPerformable, IEnumerator> currentPerformables;
    private IPerformable currentPerformable;
    private IEnumerator currentEnumerator;
    private readonly List<Intel> visibleEnemies = new List<Intel>();
    private GradientValue perceptionDelayGradient = new GradientValue(0.2f, 5f);
    private float currentAlertness = 0;
    private float lookAroundAfter = 0;


    //getters/setters
    public Body Body { get { return body; } }
    protected abstract float SightRange { get; }
    public float SightRadius { get { return SightRange * 0.75f; } }
    public List<Intel> VisibleEnemies { get { return new List<Intel>(visibleEnemies); } }
    private float PerceptionDelay { get { return perceptionDelayGradient.Lerp(currentAlertness); } }
    public ActivityState CurrentState { get { return currentState; } }


    //constuctors
    public Mind(Body body)
    {
        this.body = body;
        body.UpdateEvent += ReceiveUpdates;
    }


    //private methods
    protected virtual void ReceiveUpdates(object senter, EventArgs e)
    {
        ManagePerception();
        ManagePerformable();
        Update();
    }

    private void ManagePerformable()
    {
        IPerformable newDecision;

        if (decisionSource == null)
            newDecision = GetDecisions();
        else
            newDecision = decisionSource.GetDecisions();

        if(newDecision != currentPerformable)
        {
            currentPerformable = newDecision;
            currentEnumerator = newDecision.Perform();
        }

        if(currentEnumerator != null && !currentEnumerator.MoveNext())
        {
            currentPerformable = null;
            currentEnumerator = null;
        }
    }

    private void ManagePerception()
    {
        if(Time.time > lookAroundAfter)
        {
            lookAroundAfter = Time.time + PerceptionDelay;
            LookAround();
        }
    }

    private void LookAround()
    {
        visibleEnemies.Clear();
        var inRange = Physics.OverlapSphere(body.CameraBone.position, SightRange);//Physics.OverlapCapsule(body.CameraBone.position, body.CameraBone.forward * SightRange, SightRadius, Physics.AllLayers, QueryTriggerInteraction.Collide);
        foreach (var col in inRange)
        {
            var current = col.GetComponentInParent<ISpawnable>();
            if(current != null && !object.ReferenceEquals(current, body))
                visibleEnemies.Add(new Intel(body.gameObject, current));
        }
        visibleEnemies.Distinct();
        visibleEnemies.Sort();
    }

    protected virtual void Update()
    {

    }

    //public methods
    public abstract IPerformable GetDecisions();

    public void OverrideDecisionMaker(IDecisionMaker newDecisionSource)
    {
        decisionSource = newDecisionSource;
    }

    public IDecisionMaker GetCurrentDecisionMaker()
    {
        return decisionSource;
    }



    public virtual Relationship GetRelationship(Mind other)
    {
        //determines if another being is a friend, enemy, or whatever.

        //list of racial relationships from mind // static method implemented by inheriting specific races mind
        //list of faction relationships
        //list of personal relationships

        var result = new Relationship(0, 0, false, false);

        /*
        if (other.JoinedFaction != JoinedFaction)
            result.FriendshipLevel = -1;
        

        return result;
    }


}



    */






































/*
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



//lookForResources (seekToAcquirePerformable(list<Type> itemTypes, )
//  isTheftAllowed      // stealing resource from another
//  isHuntingAllowed    // fighting creatures for the resource of their bodies
//  isScavengingAllowed // dead bodies and the like
//  isForagingAllowed   // pick it off non sentient foliage
//  isShoppingAllowed   // just buy it


//forage for resources
//hunt for resources
//scavenge for resources
//shop for resources
//burgle for resources



//inheriting mind contains personality traits such as agressiveness, favorite activities, priorities, favorite foods
//by default inheriting mind makes decisions based on these stats in the method IPerformable GetDecisions(). an override mind can either override these traits,
//and let the inheritinng mind continue to make decisions, or intercept the decisions to filter some out, or override all
//the decisions using personality traits or not
//personality inherits from mind and implements IDecisionMaker
public abstract class Mind : IDecisionMaker
{
    //this is where behavior goes
    Body _body;
    public Body body { get { return _body; } }
    float _sightRadius = 50;
    public float sightRadius
    {
        get { return _sightRadius; }
    }
    float _sightArc = 120;
    public float sightArc
    {
        get { return _sightArc; }
    }

    Faction joinedFaction;
    public Faction JoinedFaction { get { return joinedFaction; } set { joinedFaction = value; } }

    Mind mother;
    Mind father;
    List<Mind> siblings;
    List<Mind> spouses;
    List<Mind> children;
    List<Mind> friends;
    //extendedFamily is mother/father/spouses' lists

    public abstract IPerformable GetDecisions();
    IDecisionMaker _overrideDecisionMaker;
    public IDecisionMaker overrideDecisionMaker { get { return _overrideDecisionMaker; } }
    Func<IPerformable> decisionSource
    {
        get
        {
            Func<IPerformable> result = GetDecisions;
            if (overrideDecisionMaker != null)
                result = overrideDecisionMaker.GetDecisions;
            return result;
        }
    }
    
    public void OverrideDecisionMaking(IDecisionMaker newOverrideSource)
    {
        _overrideDecisionMaker = newOverrideSource;
    }
    
    
    Vector3 _averageWeightedEnemyPosition;
    public Vector3 averageWeightedEnemyPosition { get { return _averageWeightedEnemyPosition; } }
    //need an average weighted ally position so allies dont spread so much
    //need to create a battleLines mesh representing the no mans land between allies and enemies, or the minimum safe distance from enemies. Perhaps employ 2d colliders?

    public List<BeingIntel> visibleEnemies { get { return _visibleEnemies; } }
    public List<BeingIntel> visibleAllies { get { return _visibleAllies; } }
    public List<BeingIntel> allBeingsInSightRange { get { return _allBeingsInSightRange; } }

    readonly List<BeingIntel> _visibleEnemies = new List<BeingIntel>();
    readonly List<BeingIntel> _visibleAllies = new List<BeingIntel>();
    readonly List<BeingIntel> _allBeingsInSightRange = new List<BeingIntel>();

    //beings form clusters, which are groups of beings that attack as one, or work as one etc. Attack logic treats these as one big being (unimplemented)
    IBody prevBody;
    public IBody body
    {
        get
        {
            return body.body;
        }
    }

    bool hasBody { get { return body == null; } }

    public Mind(Body being, UpdateRegistrar updateRegistrationMethod)
    {
        if (being == null || updateRegistrationMethod == null)
            throw new ArgumentNullException("a Mind must be provided with a valid Being, and UpdateRegistrar!");
        updateRegistrationMethod(Update);
        this._body = being;
        foreach (var ability in defaultMindAbilities)
        {
            UnlockAbility(ability);
            ActivateAbility(ability);
        }
        _averageWeightedEnemyPosition = being.transform.position;
        SetActivePeriods(defaultActivePeriodStart, defaultWorkPeriodLengthNormalized, defaultrecreationPeriodLengthNormalized, true);
        ManageBody();
    }

    protected virtual void Update()
    {
        ManageBody();
        ManageAbilities();
        ManagePerformable();
        UpdateMindStats();
        Perform(decisionSource());
    }
    void ManageBody()
    {
        var currentBody = body;
        if (prevBody != currentBody)
        {
            if (prevBody != null)
            {
                foreach (var ability in prevBody.unlockedBodyAbilities)
                {
                    DeactivateAbility(ability);
                    RemoveAbility(ability);
                }
            }
            if (currentBody != null)
            {
                foreach (var ability in currentBody.defaultBodyAbilities)
                {
                    UnlockAbility(ability);
                    ActivateAbility(ability);
                }
            }
        }
        prevBody = currentBody;
    }
    public void UpdateIntel()
    {
        UpdateNearbyResourcesIntel();
        UpdateNearbyBeingsIntel();
    }

    Dictionary<ItemType, List<ResourceIntel>> _allResourcesInSightRange = Item.GetItemTypeDictionary<List<ResourceIntel>>();
    public Dictionary<ItemType, List<ResourceIntel>> allResourcesInSightRange { get { return _allResourcesInSightRange; } }
    public void UpdateNearbyResourcesIntel()
    {
        _allResourcesInSightRange = Resource.GetIntelOnAllResourcesInSightRange(body);
    }


    public void UpdateNearbyBeingsIntel()
    {
        visibleEnemies.Clear();
        visibleAllies.Clear();
        allBeingsInSightRange.Clear();

        allBeingsInSightRange.AddRange(body.GetIntelOnAllBeingInSightRange());
        //allBeingsInSightRange.Sort();

        var totalWeights = 0f;
        _averageWeightedEnemyPosition = Vector3.zero;
        foreach (var inRangeBeing in allBeingsInSightRange)
        {
            if (inRangeBeing.isVisible)
            {
                if (inRangeBeing.relationship.FriendshipLevel < 0)
                {
                    visibleEnemies.Add(inRangeBeing);
                    var weight = 1f / (0.001f + 500 * inRangeBeing.distance);

                    weight *= weight;
                    totalWeights += weight;
                    _averageWeightedEnemyPosition += inRangeBeing.subject.transform.position * weight;
                }
                if (inRangeBeing.relationship.FriendshipLevel > 0)
                    visibleAllies.Add(inRangeBeing);
            }
        }
        if (allBeingsInSightRange.Count > 0)
            _averageWeightedEnemyPosition /= totalWeights;
        else
            _averageWeightedEnemyPosition = Vector3.negativeInfinity;


        Debug.DrawRay(averageWeightedEnemyPosition, 100 * Vector3.up, Color.magenta, 0.1f);
    }

    //the main access to perform should be in mind. From there mind should check if body or
    //ability manager is locked into something. From there it can direct the "Perform" directive to its appropriate location
    //abilityManager should really be absorbed into mind





    /// <summary>
    /// gets this beings relationship to another
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public Relationship GetRelationship(Body other)
    {
        //determines if another being is a friend, enemy, or whatever.

        //list of racial relationships from mind // static method implemented by inheriting specific races mind
        //list of faction relationships
        //list of personal relationships

        var result = new Relationship(0, 0, false, false);

        if (other.mind.JoinedFaction != JoinedFaction)
            result.FriendshipLevel = -1;

        return result;
    }


    public float MaxDamageAgainstEnemy(Body enemy, Vector3 enemyPosition)
    {
        if (enemy == null)
            return 0;
        //this gameobject could be destroyed, and its being accessed via reference
        float result = 0;
        if (this != null && body != null)
        {
            foreach (var ability in GetUnlockedAbilities())
            {
                IAttack a = ability as IAttack;
                float abilityAttackDamage = 0;
                if (a != null)
                    abilityAttackDamage = a.PreviewAttackDamage(enemyPosition, enemy);
                result = (abilityAttackDamage > result) ? abilityAttackDamage : result;
            }
        }
        return result;
    }

    public virtual bool IsVisible(RelativePositionInfo other)
    {
        return (other.distance <= sightRadius && other.angle < sightArc) || (other.distance < 5f);
    }



    public virtual List<BeingIntel> GetAllBeingsVisibleToSelf()
    {
        return new List<BeingIntel>(allBeingsInSightRange.Where(delegate (BeingIntel b) { return b.isVisible; }));
    }







    //************************************************************
    //************************************************************
    //************************************************************
    //this section is for methods related to performing Performables
    //performables may be being performed either in the ibody or abilityManager
    public bool isLockedInAction { get { return isLockedInAbility || body == null || body.isLockedInMovement; } }
    IEnumerator currentPerformableEnumerator;
    IPerformable _currentPerformable;
    public IPerformable currentPerformable { get { return _currentPerformable; } }
    public bool isPerformingPerformable { get { return !(currentPerformable == null || currentPerformableEnumerator == null); } }// is body performing, is mind performing?

    public ActivityState currentActivityState { get { return (currentPerformable != null) ? currentPerformable.activityType : ActivityState.Nothing; } }
    public bool AbortPerformable()//did this successfully stop all performances?
    {
        var result = true;
        if (_currentPerformable != null)
            _currentPerformable.Abort();
        _currentPerformable = null;
        currentPerformableEnumerator = null;
        return result;
    }

    public void Perform(IPerformable toPerform)
    {
        if (toPerform == null || toPerform.isComplete || toPerform == currentPerformable)
            return;
        //Debug.Log("perform: " + being.gameObject.name + toPerform);
        if (!toPerform.isComplete)
        {
            if (toPerform is Ability)
                Perform(toPerform as Ability);
            else
            {
                body.StartCoroutine(_Perform(toPerform));
            }
        }
    }

    public void Perform<T>() where T : IAbility
    {
        var ability = GetActivatedAbility<T>();
        Perform(ability);
    }

    IEnumerator _Perform(IPerformable toPerform)
    {
        while (isLockedInAction)
            yield return null;
        AbortAllActions();
        _currentPerformable = toPerform;
        currentPerformableEnumerator = toPerform.Perform();
        currentPerformableEnumerator.MoveNext();
    }

    void ManagePerformable()
    {
        if (currentPerformableEnumerator != null && (currentPerformableEnumerator.MoveNext() == false || currentPerformable.isComplete))
        {
            _currentPerformable = null;
            currentPerformableEnumerator = null;
        }
    }

    public bool AbortAllActions()
    {
        var result = true;
        //abort navigation
        if (body != null)
            result &= body.AbortMovement();
        //abort performables
        result &= AbortPerformable();
        //abort abilities
        result &= AbortAbilitiesInProgress();
        return result;
    }

    //************************************************************
    //************************************************************
    //************************************************************
    //************************************************************

    //max number of in-game days of each stat Mind can hold
    public abstract float wakefulnessMax { get; }
    public abstract float excitementMax { get; }
    public abstract float spiritualityMax { get; }
    public abstract float socializationMax { get; }
    public abstract float caloriesMax { get; }
    public abstract float bloodMax { get; }
    //number of in-game days of each stat Mind has left
    public float wakefulness { get { return _wakefulness; } }
    public float excitement { get { return _excitement; } }
    public float spirituality { get { return _spirituality; } }
    public float socialization { get { return _socialization; } }
    public float calories { get { return _calories; } }
    public float blood { get { return _blood; } }
    public bool isAwake { get { return _isAwake; } }
    
    float _wakefulness = float.MaxValue;//decreases when awake, depletes quicker when performing taxing tasks, increases 1.5x speed when sleeping
    float _excitement = float.MaxValue;//depletes awake or asleep. Tedious work takes chunks from this. recreation activities increase this
    float _spirituality = float.MaxValue;//depletes awake or asleep. recreation activities increase this. Tedious Work or seedy activities takes chunks from this
    float _socialization = float.MaxValue;//depletes when awake. increases when working or playing together
    float _calories = float.MaxValue;//num days calories will last. ie 1 calorie is enough for 1 day. should go in body
    float _blood = float.MaxValue;//reaching 0 blood and being passes out
    bool _isAwake = true;



    float _activePeriodStart = 0;
    float _workPeriodLength = 0;
    float _recreationPeriodLength = 0;

    /// <summary>
    /// in hours
    /// </summary>
    /// <param name="activePeriodStart"></param>
    /// <param name="workPeriodLength"></param>
    /// <param name="recreationPeriodLength"></param>
    public void SetActivePeriods(float activePeriodStart, float workPeriodLength, float recreationPeriodLength, bool isNormalizedLength)
    {
        if (isNormalizedLength)
        {
            workPeriodLength *= GameTime.DaysToHours;
            recreationPeriodLength *= GameTime.DaysToHours;
        }
        this._activePeriodStart = Mathf.Clamp(activePeriodStart, 0f, GameTime.DaysToHours) % GameTime.DaysToHours;
        this._workPeriodLength = Mathf.Clamp(workPeriodLength, 0f, GameTime.DaysToHours);
        this._recreationPeriodLength = Mathf.Clamp(recreationPeriodLength, 0f, GameTime.DaysToHours - workPeriodLength);
    }

    protected abstract float defaultActivePeriodStart { get; }
    protected abstract float defaultWorkPeriodLengthNormalized { get; }
    protected abstract float defaultrecreationPeriodLengthNormalized { get; }

    //active period, recreation period, rest priod
    public float activePeriodStart { get { return _activePeriodStart; } }//time of active period, and work start

    public float workPeriodStart { get { return activePeriodStart; } }//same as active period start
    public float workPeriodLength { get { return _workPeriodLength; } }//how long the work period lasts
    public float workPeriodEnd { get { return (workPeriodStart + workPeriodLength) % GameTime.DaysToHours; } }//time of work period start

    public float recreationPeriodStart { get { return workPeriodEnd; } }//same as work period end
    public float recreationPeriodLength { get { return _recreationPeriodLength; } }//how long the recreation period lasts
    public float recreationPeriodEnd { get { return (recreationPeriodStart + recreationPeriodLength) % GameTime.DaysToHours; } }//time of recreation period end

    public float sleepPeriodStart { get { return recreationPeriodEnd; } }//same as recreation period end
    public float sleepPeriodLength { get { return Mathf.Clamp(GameTime.DaysToHours - workPeriodLength - recreationPeriodLength, 0f, float.MaxValue); } }//how long the sleepationn period lasts
    public float sleepPeriodEnd { get { return (sleepPeriodStart + sleepPeriodLength) % GameTime.DaysToHours; } }//time of sleep period end

    public bool IsWorkPeriod { get { return GameTime.IsBetweenHours(GameTime.Hour, workPeriodStart, workPeriodEnd); } }
    public bool IsRecreationPeriod { get { return GameTime.IsBetweenHours(GameTime.Hour, recreationPeriodStart, recreationPeriodEnd); } }
    public bool IsSleepPeriod { get { return GameTime.IsBetweenHours(GameTime.Hour, sleepPeriodStart, sleepPeriodEnd); } }

    void UpdateMindStats()
    {
        _wakefulness -= GameTime.DeltaTimeGameDays;
        _excitement -= GameTime.DeltaTimeGameDays;
        _spirituality -= GameTime.DeltaTimeGameDays;
        _socialization -= GameTime.DeltaTimeGameDays;
        _calories -= GameTime.DeltaTimeGameDays;
        _blood += GameTime.DeltaTimeGameDays;

        if (currentPerformable != null)
        {
            _wakefulness += currentPerformable.deltaWakefulness * GameTime.DeltaTimeGameDays * ((currentActivityState == ActivityState.Sleep && IsSleepPeriod) ? 2f : 1f);//decreases when awake, depletes quicker when performing taxing tasks, increases 1.5x speed when sleeping
            _excitement += currentPerformable.deltaExcitement * GameTime.DeltaTimeGameDays;//depletes awake or asleep. Tedious work takes chunks from this. recreation activities increase this
            _spirituality += currentPerformable.deltaSpirituality * GameTime.DeltaTimeGameDays;//depletes awake or asleep. recreation activities increase this. Tedious Work or seedy activities takes chunks from this
            _socialization += currentPerformable.deltaSocialization * GameTime.DeltaTimeGameDays;//depletes when awake. increases when working or playing together
            _calories += currentPerformable.deltaCalories * GameTime.DeltaTimeGameDays;//num days calories will last.
            _blood += currentPerformable.deltaBlood * GameTime.DeltaTimeGameDays;//reaching 0 blood and being passes out
            _isAwake = !currentPerformable.isSleepActivity;
        }

        _wakefulness = Mathf.Clamp(wakefulness, 0, wakefulnessMax);
        _excitement = Mathf.Clamp(excitement, 0, excitementMax);
        _spirituality = Mathf.Clamp(spirituality, 0, spiritualityMax);
        _socialization = Mathf.Clamp(socialization, 0, socializationMax);
        _calories = Mathf.Clamp(calories, 0, caloriesMax);
        _blood = Mathf.Clamp(blood, 0, bloodMax);
    }




    //************************************************************
    //************************************************************
    //************************************************************
    //************************************************************




    /*
	 * so it turns out animation events SUCK so this is how itll work:
	 * an animation will be looked up based on the Type of the ability, gotten using GetType();
	 * the name of the type will be used to look up the animation in the animation controller
	 * the animation length will be gathered from the animation
	 * to trigger an animatin, anim.CrossFade() will be used
	 * the current state of the RuntimeAnimationController will have to be used to determine the ACTUAL
	 * length of the animation, after blending and crossfading is taken into account
	 * there will be properties called "warmUpStart" and "warmUpEnd" which will be floats between 0 and 1
	 * these properties will be the percentage of elapsed time to total time at which the events will fired
	 * the properties will have to be hard coded by the programmer. 
	 ///

    public bool isPerformingAbility
    {
        get
        {
            bool result = false;
            foreach (var ability in activeAbilities.Keys)
            {
                if (ability.isPerforming)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }
    }
    public bool isWindingUp
    {
        get
        {
            bool result = false;
            foreach (var ability in activeAbilities.Keys)
            {
                result |= ability.isWindingUp;
            }
            return result;
        }
    }
    public bool isLockedInAbility
    {
        get
        {
            bool result = false;
            foreach (var ability in activeAbilities.Keys)
            {
                result |= ability.isLockedInAbility;
            }
            return result;
        }
    }
    public bool isInActiveFrames
    {
        get
        {
            bool result = false;
            foreach (var ability in activeAbilities.Keys)
            {
                result |= ability.isInActiveFrames;
            }
            return result;
        }
    }
    public bool isInRecoveryFrames
    {
        get
        {
            bool result = false;
            foreach (var ability in activeAbilities.Keys)
            {
                result |= ability.isInRecoveryFrames;
            }
            return result;
        }
    }

    readonly HashSet<IMindAbility> _unlockedMindAbilities = new HashSet<IMindAbility>();
    public HashSet<IMindAbility> unlockedMindAbilities { get { return new HashSet<IMindAbility>(_unlockedMindAbilities); } }
    HashSet<IAbility> unlockedAbilities
    {
        get
        {
            var result = new HashSet<IAbility>(unlockedMindAbilities.OfType<IAbility>());
            if (body != null)
                result.UnionWith(body.unlockedBodyAbilities.OfType<IAbility>());
            return result;
        }
    }
    HashSet<IAbility> defaultAbilities
    {
        get
        {
            var result = new HashSet<IAbility>(defaultMindAbilities.OfType<IAbility>());
            if (body != null)
                result.UnionWith(body.defaultBodyAbilities.OfType<IAbility>());
            return result;
        }
    }
    //this is for activated abilities. Abilities may stay activated indefinitely or deactivate themselves
    readonly Dictionary<IAbility, IEnumerator> activeAbilities = new Dictionary<IAbility, IEnumerator>();
    //every frame go through every ability and call hasnext. if hasnext == false, remove the ability from the list
    void ManageAbilities()
    {
        var toRemove = new List<IAbility>();
        foreach (var ability in activeAbilities.Keys)
        {
            if (activeAbilities[ability] == null || activeAbilities[ability].MoveNext() == false)
                toRemove.Add(ability);
        }
        foreach (var ability in toRemove)
        {
            activeAbilities.Remove(ability);
        }
    }

    public bool AbortAbilitiesInProgress()
    {
        var result = true;
        foreach (var ability in activeAbilities.Keys)
        {
            if (ability.isPerforming)
                ability.Abort();
            //result  = result & ability.Abort();//some abilities cant be aborted
        }
        return result;
    }


    public bool Perform(IAbility abilityToPerform)
    {
        //if told to perform an ability that is not in its activatedDictionary, it is added to that dictionary
        var result = false;
        if (!isLockedInAbility || abilityToPerform.canInterrupt)
        {
            result = AbortAbilitiesInProgress();
            if (result)
            {
                if (!activeAbilities.ContainsKey(abilityToPerform))
                    ActivateAbility(abilityToPerform);
                abilityToPerform.Perform();
            }
            //must check if ability is in activeAbilities
            //must check every frame for isComplete to manually abort
        }
        return result;
    }



    public void UnlockAbility(IAbility abilityToUnlock)
    {
        if (abilityToUnlock != null && !unlockedAbilities.Contains(abilityToUnlock))
        {
            if (abilityToUnlock is IMindAbility)
            {
                if (unlockedMindAbilities.Any(delegate (IMindAbility currentAbility) { return currentAbility.GetType() == abilityToUnlock.GetType(); }))
                {
                    var matchingAbility = unlockedMindAbilities.FirstOrDefault(delegate (IMindAbility currentAbility) { return currentAbility.GetType() == abilityToUnlock.GetType(); });
                    Debug.Log("ability of type '" + abilityToUnlock.GetType() + "' is alredy unlocked! replacing current ability(" + matchingAbility + ") with new ability (" + abilityToUnlock + ")");
                    DeactivateAbility(matchingAbility);
                    ActivateAbility(abilityToUnlock);
                }
                unlockedMindAbilities.Add(abilityToUnlock as IMindAbility);
            }
            else if (abilityToUnlock is IBodyAbility && body != null)
            {
                body.UnlockAbility(abilityToUnlock as IBodyAbility);
            }
        }
    }

    public void RemoveAbility(IAbility abilityToRemove)
    {
        if (abilityToRemove != null && unlockedAbilities.Contains(abilityToRemove))
        {
            if (abilityToRemove is IMindAbility)
            {
                unlockedMindAbilities.Remove(abilityToRemove as IMindAbility);
            }
            else if (abilityToRemove is IBodyAbility && body != null)
            {
                body.RemoveAbility(abilityToRemove as IBodyAbility);
            }
        }
    }


    public void ActivateAbility(IAbility abilityToActivate)
    {
        if (abilityToActivate == null)
            return;
        DeactivateAbility(abilityToActivate);
        activeAbilities.Add(abilityToActivate, abilityToActivate.ActivateAbility());
    }

    public void DeactivateAbility(IAbility abilityToDeactivate)
    {
        if (abilityToDeactivate == null)
            return;
        if (activeAbilities.ContainsKey(abilityToDeactivate))
        {
            if (abilityToDeactivate.isPerforming)
                abilityToDeactivate.Abort();
            activeAbilities.Remove(abilityToDeactivate);
        }
    }

    public List<IAbility> GetActivatedAbilities()
    {
        return new List<IAbility>(activeAbilities.Keys);
    }

    public List<IAbility> GetUnlockedAbilities()
    {
        return new List<IAbility>(unlockedAbilities);
    }

    public T GetActivatedAbility<T>() where T : IAbility
    {
        var result = activeAbilities.Keys.First((IAbility a) => { return a is T; });
        return (T)result;
    }

    public T GetUnlockedAbility<T>() where T : IAbility
    {
        var result = unlockedAbilities.First((IAbility a) => { return a is T; });
        return (T)result;
    }

    public void DeactivateAllAbilities()
    {
        AbortAbilitiesInProgress();
        activeAbilities.Clear();
    }


}

//priorities what the citizen wants to do
//foreach enemy in sight, you have to decide fight or flight
//public enum Priorities
*/

































