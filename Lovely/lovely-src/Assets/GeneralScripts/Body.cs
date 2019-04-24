using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// bodies are the physical representation of creatures. As such Body : UnifiedController because bodies can move and animate.
/// The diference is that every Body has a Mind which controlls it. Body regulates bodyStats like hunger, damage, stamina.
/// Body has a list of abilities associated with that body, but mind is the portion that can activate the abilities, triggering them in body.
/// think of body as a puppet that can be manipulated by either a performable or an ability. DecisionMakers are basically coreographers who 
/// organize all the performables into a logical dance.
/// so abilities should be triggered by performables; performables should be chosen by decision makers; the performables chosen by decision 
/// makers should then manipulate body. Though it is possible to just call the method to activate a performable or an ability in itself, this should not be done.
/// the same goes for moving body manually. If manual control is desired, body should be eschewed in favor of unified controller
/// 
/// body loads the prefab of its specific model and avatar and override controller
/// body also create a new mind of the appropriate type
/// </summary>

public abstract class Body : UnifiedController, ISpawnable
{
    //seperate into bodyBase and body

    public GameObject GameObject { get { return (this == null) ? null : gameObject; } }
    public Transform Transform { get { return (this == null) ? null : transform; } }
    public Bounds Bounds { get { if (this == null) return new Bounds(); else if (bodyMesh == null) return new Bounds(transform.position, Vector3.zero); else return bodyMesh.bounds; } }

    [ShowOnly]
    [SerializeField]
    private float blood = float.MaxValue;//reaching 0 blood and being passes out
    [ShowOnly]
    [SerializeField]
    private float stamina = float.MaxValue;//max is further limited by a penalty value, based on other stats
    [ShowOnly]
    [SerializeField]
    private float calories = float.MaxValue;//num days calories will last. ie 1 calorie is enough for 1 day.
    [ShowOnly]
    [SerializeField]
    private float bloodMax = float.MaxValue;//reaching 0 blood and being passes out
    [ShowOnly]
    [SerializeField]
    private float staminaMax = float.MaxValue;//max is further limited by a penalty value, based on other stats
    [ShowOnly]
    [SerializeField]
    private float caloriesMax = float.MaxValue;//num days calories will last. ie 1 calorie is enough for 1 day.
    [ShowOnly]
    [SerializeField]
    private int empowermentLevel = 0;
    [ShowOnly]
    [SerializeField]
    private ItemPack backpack = new ItemPack();
    
    public abstract PerceivingMind Mind { get; }


    public float BloodMax { get { return bloodMax; } protected set { bloodMax = value; } }
    public float StaminaMax { get { return staminaMax; } protected set { staminaMax = value; } }
    public float CaloriesMax { get { return caloriesMax; } protected set { caloriesMax = value; } }
    //public float caloriesMax { get { return _caloriesMax; } protected set { _caloriesMax = value; } }
    //public float bloodMax { get { return _bloodMax; } protected set { _bloodMax = value; } }
    public abstract Gender Gender { get; }
    public abstract string PrefabName { get; }
    private readonly CharacterAbilities characterAbilities = new CharacterAbilities();
    public CharacterAbilities CharacterAbilities { get { return characterAbilities; } }
    public ItemPack Backpack { get { return backpack; } protected set { backpack = value; } }
    public float Calories { get { return calories; } protected set { calories = value; } }
    public float Blood { get { return blood; } protected set { blood = value; } }
    public float Stamina { get { return stamina; } protected set { stamina = value; } }


    SkinnedMeshRenderer bodyMesh;

    public event CharDeathEventHandler CharDeathEvent;


    //\/////////////////////////////////////////////////////////////////////////////////////////////
    //events and override events
    protected override void Awake()
    {
        base.Awake();
        TrackedComponent<Body>.Track(this);

        InitializeDamageBoxes();

        blood = BloodMax;
        stamina = StaminaMax;
        calories = CaloriesMax;

        if (Gender == Gender.Male)
            gameObject.name = Names.maleNames.Random();
        if (Gender == Gender.Female)
            gameObject.name = Names.maleNames.Random();

    }

    protected override void Update()
    {
        base.Update();

        if (bodyMesh == null)
            bodyMesh = GetComponentInChildren<SkinnedMeshRenderer>();
        if (bodyMesh != null && bodyMesh.material.HasProperty("_Color"))
        {
            var baseColor = bodyMesh.sharedMaterial.color;
            var modifier = Color.Lerp(Color.magenta, Color.green, blood / 100f);
            var newColor = Color.Lerp(modifier, baseColor, 0.5f);
            bodyMesh.material.color = (blood <= 0f) ? Color.red : newColor;
        }
        UpdateBodyStats();
        UpdateEmpowerment();
    }
    
    protected virtual void OnCharDeath(Body sender, CharDeathEventArgs e)
    {
        if (CharDeathEvent != null)
            CharDeathEvent(this, e);

        Destroy(this.gameObject);///return this to pool
    }
    //\/////////////////////////////////////////////////////////////////////////////////////////////
    //\/////////////////////////////////////////////////////////////////////////////////////////////

    #region [[[ empowerment and events ]]]

    public int EmpowermentLevel { get { return empowermentLevel; } }
    public const int maxEmpowermentLevel = 3;//in sec
    public const float empowerTime = 7;//in sec
    private float depowerAfter = 0;//time to reset empowerment after
    public event EmpowerChangeEventHandler EmpowermentChangeEvent;
    private PowerUpEffect powerUpEffects;

    private void UpdateEmpowerment()
    {
        if (empowermentLevel != 0 && Time.time > depowerAfter)
        {
            ConsumeEmpowerment();
        }
    }
    public void Empower()
    {
        var newEmpowermentLevel = (empowermentLevel + 1) % (maxEmpowermentLevel + 1);
        if(newEmpowermentLevel != empowermentLevel)
            OnEmpowermentChange(this, new EmpowerChangeEventArgs(empowermentLevel, newEmpowermentLevel));
        depowerAfter = Time.time + empowerTime;
    }

    public void ConsumeEmpowerment()
    {
        if(empowermentLevel != 0)
            OnEmpowermentChange(this, new EmpowerChangeEventArgs(empowermentLevel, 0));
    }

    protected virtual void OnEmpowermentChange(Body sender, EmpowerChangeEventArgs e)
    {
        if (e.newPowerLevel != e.oldPowerLevel)
        {
            if (EmpowermentChangeEvent != null)
                EmpowermentChangeEvent(this, e);

            //overriding classes should modify e
            empowermentLevel = e.newPowerLevel;
            //e.finalized = true
            UpdateEmpowerVisualEffect(e);
        }
    }

    protected virtual void UpdateEmpowerVisualEffect(EmpowerChangeEventArgs e)
    {
        if (powerUpEffects == null)
        {
            powerUpEffects = GameObject.Instantiate<GameObject>(_PrefabPool.GetPrefab("PowerUpEffect").GameObject).GetComponent<PowerUpEffect>();
            powerUpEffects.transform.SetParent(transform);
            powerUpEffects.transform.localPosition = Vector3.zero;
        }
        powerUpEffects.SetPowerLevel(e.newPowerLevel);
    }

    #endregion

    //can be attacks or augments, good or bad
    public void ApplyAbilityEffects(PerceivingMind damager, float deltaHealth, AnimationClip effectAnimation)
    {
        blood += deltaHealth;
        PlayInterruptAnimation(effectAnimation);
        if(blood <= 0)
            OnCharDeath(this, new CharDeathEventArgs());
    }

    public bool GetMaintinenceAssignment(Body being, ref IPerformable assignment)
    {
        var assignedPerformable = true;
        assignment = new EmptyPerformable();
        return assignedPerformable;
    }

    public bool GetRestAssignment(Body being, ref IPerformable assignment)
    {
        var assignedPerformable = true;
        //sleep until its no longer sleep period or mind enters an emergency state
        assignment = new SleepPerformable(this.Mind, () => { return this.Mind.IsSleepPeriod == false || this.Mind.IsInEmergency; });
        return assignedPerformable;
    }

    private void UpdateBodyStats()
    {
        calories -= GameTime.DeltaTimeGameDays;
        blood += GameTime.DeltaTimeGameDays;
        if (Mind.CurrentPerformable != null)
        {
            calories += Mind.CurrentPerformable.DeltaCalories * GameTime.DeltaTimeGameDays;//num days calories will last.
            blood += Mind.CurrentPerformable.DeltaBlood * GameTime.DeltaTimeGameDays;//reaching 0 blood and being passes out
        }
        calories = Mathf.Clamp(calories, 0, CaloriesMax);
        blood = Mathf.Clamp(blood, 0, BloodMax);
        anim.SetFloat("BreathingLabor", 1f - (stamina / 100));
    }

    private readonly List<CapsuleCollider> hurtBoxes = new List<CapsuleCollider>();
    private readonly Dictionary<HitBoxType, CapsuleCollider> hitBoxes = new Dictionary<HitBoxType, CapsuleCollider>();

    private void InitializeDamageBoxes()
    {
        foreach (var collider in transform.GetComponentsInChildren<CapsuleCollider>())
        {
            if (collider.gameObject.layer == LayerMask.NameToLayer("HurtBox"))
            {
                hurtBoxes.Add(collider);
            }
            else if (collider.gameObject.layer == LayerMask.NameToLayer("HitBox"))
            {
                collider.enabled = false;
                switch (collider.gameObject.name)
                {
                    case "hand.R":
                        hitBoxes.Add(HitBoxType.HandR, collider);
                        break;
                    case "hand.L":
                        hitBoxes.Add(HitBoxType.HandL, collider);
                        break;
                    case "foot.R":
                        hitBoxes.Add(HitBoxType.FootR, collider);
                        break;
                    case "foot.L":
                        hitBoxes.Add(HitBoxType.FootL, collider);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    //todo: void GetCollisionBoxByName


    //make these based on string boneName
    public void SetHurtBoxActiveState(bool isActive)
    {
        foreach (var hurtBox in hurtBoxes)
        {
            hurtBox.enabled = isActive;
        }
    }

    public CapsuleCollider SetHitBoxActiveState(HitBoxType hitBoxType, bool isActive)
    {
        CapsuleCollider result = null;
        if (hitBoxes.ContainsKey(hitBoxType))
        {
            result = hitBoxes[hitBoxType];
            hitBoxes[hitBoxType].enabled = isActive;
        }
        return result;
    }

    public List<CapsuleCollider> SetHitBoxActiveState(bool isActive)
    {
        List<CapsuleCollider> result = new List<CapsuleCollider>();
        foreach (var hitBoxType in hitBoxes.Keys)
        {
            result.Add(hitBoxes[hitBoxType]);
            hitBoxes[hitBoxType].enabled = isActive;
        }
        return result;
    }


    private enum CollisionBoxType
    {
        None = 0,
        Physics,
        HitBox,
        HurtBox,
    }
}
