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

public abstract partial class Body : UnifiedController, ISpawnable
{
    [ShowOnly]
    [SerializeField]
    private float health;
    [ShowOnly]
    [SerializeField]
    private float stamina;
    [ShowOnly]
    [SerializeField]
    private int empowermentLevel = 0;

    public const int maxEmpowermentLevel = 3;//in sec
    public const float empowerTime = 7;//in sec
    private float depowerAfter = 0;//time to reset empowerment
    public event EmpowerChangeEventHandler EmpowermentChangeEvent;
    private PowerUpEffect powerUpEffects;

    public int EmpowermentLevel { get { return empowermentLevel; } }
    public abstract Mind Mind { get; }
    public abstract float MaxHealth { get; }
    public abstract float MaxStamina { get; }
    public abstract Gender Gender { get; }
    public abstract string PrefabName { get; }
    public abstract CharacterAbilities CharacterAbilities { get; }
    SkinnedMeshRenderer bodyMesh;




    //\/////////////////////////////////////////////////////////////////////////////////////////////
    //events and override events
    protected override void Awake()
    {
        base.Awake();
        InitializeDamageBoxes();

        health = MaxHealth;
        stamina = MaxStamina;

        if (Gender == Gender.Male)
            gameObject.name = Names.maleNames.Random();
        if (Gender == Gender.Female)
            gameObject.name = Names.maleNames.Random();

    }
    
    protected override void Update()
    {
        base.Update();
        if(bodyMesh == null)
            bodyMesh = GetComponentInChildren<SkinnedMeshRenderer>();

        if(bodyMesh != null && bodyMesh.material.HasProperty("_Color"))
        {
            var baseColor = bodyMesh.sharedMaterial.color;
            var modifier = Color.Lerp(Color.magenta, Color.green, health / 100f);
            var newColor = Color.Lerp(modifier, baseColor, 0.5f);
            bodyMesh.material.color = (health <= 0f) ? Color.red : newColor;
        }
        anim.SetFloat("BreathingLabor", 1f - (stamina / 100));

        if ( empowermentLevel != 0 && Time.time > depowerAfter)
        {
            ConsumeEmpowerment();
        }
    }

    protected virtual void OnEmpowermentChange(Body sender, EmpowerChangeEventArgs e)
    {
        if( e.newPowerLevel != e.oldPowerLevel)
        {
            if (EmpowermentChangeEvent != null)
                EmpowermentChangeEvent(this, e);

            //overriding classes should modify e
            empowermentLevel = e.newPowerLevel;
            //e.finalized = true
            UpdateEmpowerVisualEffect(this, e);
        }
    }
    //\/////////////////////////////////////////////////////////////////////////////////////////////
    //\/////////////////////////////////////////////////////////////////////////////////////////////

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

    protected virtual void UpdateEmpowerVisualEffect(Body sender, EmpowerChangeEventArgs e)
    {
        if (powerUpEffects == null)
        {
            powerUpEffects = GameObject.Instantiate<GameObject>(_PrefabPool.GetPrefab("PowerUpEffect").gameObject).GetComponent<PowerUpEffect>();
            powerUpEffects.transform.SetParent(transform);
            powerUpEffects.transform.localPosition = Vector3.zero;
        }
        powerUpEffects.SetPowerLevel(e.newPowerLevel);
    }
    
    public void Jump()
    {

    }
    //can be attacks or augments, good or bad
    public void ApplyAbilityEffects(Mind damager, float deltaHealth, AnimationClip effectAnimation)
    {
        health += deltaHealth;
        PlayInterruptAnimation(effectAnimation);
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
