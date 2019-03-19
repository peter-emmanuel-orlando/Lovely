using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodNovaBeam : Projectile
{ 
    private ProjectileStats projectileStats = new ProjectileStats("BloodNovaBeam");
    protected AnimationClip recoilAnimation = _AnimationPool.GetAnimation("KnockBack_Heavy");
    private GradientValue beamSize = new GradientValue(0, 30);
    private float currentTime = 0;
    private const float timeForFullLength = 0.5f;
    private float damage = 3f;

    protected override event ColliderEventHandler OnHitEvent;

    public override ProjectileStats ProjectileStats { get { return projectileStats; } }

    protected override void OnEnable()
    {

    }

    private void Update()
    {
        transform.localScale =  Vector3.one + Vector3.forward * beamSize.Lerp(currentTime / timeForFullLength);
        currentTime += Time.deltaTime;
        if (currentTime >= timeForFullLength)
            Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (OnHitEvent != null)
            OnHitEvent(gameObject, new ColliderEventArgs(other));
    }

    protected override void ApplyEffectsEveryFrame(Body hitBody)
    {
        var v = hitBody;
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
