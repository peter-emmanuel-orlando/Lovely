using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodNovaBeam : Projectile
{ 
    private ProjectileStats projectileStats = new ProjectileStats("BloodNovaBeam");
    protected AnimationClip recoilAnimation;
    private GradientValue beamLength = new GradientValue(0, 30);
    private GradientValue beamRadius = new GradientValue(1, 0);
    private float currentTime = 0;
    private const float timeForFullLength = 0.5f;
    private const float timeForFade = 0.2f;
    private const float totalTime = timeForFullLength + timeForFade;
    private float damage = 30f;

    protected override event ColliderEventHandler OnHitEvent;

    public override ProjectileStats ProjectileStats { get { return projectileStats; } }

    protected override void OnEnable()
    {

    }

    protected override void Awake()
    {
        base.Awake();
        recoilAnimation = _AnimationPool.GetAnimation("KnockBack_Heavy");
    }

    private void Update()
    {
        if (currentTime <= timeForFullLength)
        {
            transform.localScale = (Vector3.one - Vector3.forward) * beamRadius.value1 + Vector3.forward * beamLength.Lerp(currentTime / timeForFullLength);
        }
        else
        {
            var lerpFactor = (currentTime - timeForFullLength) / timeForFade;
            transform.localScale = (Vector3.one - Vector3.forward) * beamRadius.Lerp(lerpFactor) + Vector3.forward * beamLength.value2;
        }

        if (currentTime >= totalTime )
        {
            Destroy(this.gameObject);
        }
        currentTime += Time.deltaTime;
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
    }

}

public class BloodNovaBeamAttack : ProjectileAttack
{
    ProjectileStats projectileStats = new ProjectileStats("BloodNovaBeam");
    public BloodNovaBeamAttack(Body body) : base(body)
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

    protected override Vector3 SpawnedProjectileNewLocation { get { return performer.SetHitBoxActiveState(HitBoxType.HandR,false).transform.position; } }

    protected override Quaternion SpawnedProjectileNewRotation { get { return performer.transform.rotation; } }

    protected override Transform SpawnedProjectileNewParent { get { return null; } }

    protected override ScheduledAction[] OtherScheduledActions { get { return new ScheduledAction[] { }; } }

    protected override AnimationClip AbilityAnimation { get { return _AnimationPool.GetAnimation("PowerBlast"); } }


}
