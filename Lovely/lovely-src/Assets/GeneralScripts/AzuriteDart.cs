using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AzuriteDart : Projectile
{ 
    private ProjectileStats projectileStats = new ProjectileStats("AzuriteDart");
    protected AnimationClip recoilAnimation;
    private Rigidbody rb;
    private float damage = 10f;
    private float lifeTime = 10f;

    protected override event ColliderEventHandler OnHitEvent;

    public override ProjectileStats ProjectileStats { get { return projectileStats; } }

    protected override void Awake()
    {
        base.Awake();
        recoilAnimation = _AnimationPool.GetAnimation("KnockBack_Light");
        rb = GetComponentInChildren<Rigidbody>();
        if (rb == null) throw new UnityException("an AzuriteDart needs a rigidbody for colission ");
    }

    protected override void OnEnable()
    {

    }

    protected override void Start()
    {
        base.Start();
        rb.AddRelativeForce(Vector3.forward * 20, ForceMode.VelocityChange);
        Destroy(this.gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        rb.useGravity = true;
        var ps = GetComponentInChildren<ParticleSystem>();
        ps.Play();
        var al = GetComponentInChildren<AnimatedLight>();
        al.Reset();
        al.playMode = AnimatedLight.PlayMode.Loop;
        Destroy(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (OnHitEvent != null)
            OnHitEvent(gameObject, new ColliderEventArgs(other));
    }

    protected override void ApplyEffectsEveryFrame(Body hitBody)
    {

    }

    protected override void ApplyEffectsOnce(Body hitBody)
    {
        Body initiatorBody = null;
        if (initiator != null)
            initiatorBody = initiator.GetComponentInParent<Body>();

        if (hitBody != initiatorBody)
        {
            if (initiator != null)
            {
                //hitBody.TurnToFace(initiator.transform.position);
            }
            if(initiatorBody != null)
                hitBody.ApplyAbilityEffects(initiatorBody.Mind, -Mathf.Abs(damage), recoilAnimation);
            else
                hitBody.ApplyAbilityEffects(null, -Mathf.Abs(damage), recoilAnimation);
        }
        Destroy(this);
    }

}

//beamAttack class, animation is beamattack animation, animationStats come from generic T
//throw dart class, different animation, same implementation

public class AzuriteDartAttack : ProjectileAttack
{
    ProjectileStats projectileStats = new ProjectileStats("AzuriteDart");
    public AzuriteDartAttack(Body body) : base(body)
    {

    }

    public override void CastAbility()
    {
        performer.ConsumeEmpowerment();
        base.CastAbility();
    }

    public override float Range { get { throw new System.NotImplementedException(); } }

    protected override ProjectileStats ProjectileStats { get { return projectileStats; } }

    protected override float[] NormalizedProjectileSpawnTimes { get { return new float[] {0.8f }; } }

    protected override Vector3 SpawnedProjectileNewLocation { get { return performer.SetHitBoxActiveState(HitBoxType.HandL,false).transform.position; } }

    protected override Quaternion SpawnedProjectileNewRotation { get { return performer.transform.rotation; } }

    protected override Transform SpawnedProjectileNewParent { get { return null; } }

    protected override ScheduledAction[] OtherScheduledActions { get { return new ScheduledAction[] { }; } }

    protected override AnimationClip AbilityAnimation { get { return _AnimationPool.GetAnimation("ThrowKunai"); } }


}
