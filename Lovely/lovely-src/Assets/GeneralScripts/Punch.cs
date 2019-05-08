using UnityEngine;

public class Punch : PhysicalAttack
{
    private static readonly AnimationClip attackAnimation = _AnimationPool.GetAnimation("Punch_Mid_R");
    private static readonly AnimationClip recoilAnimation = _AnimationPool.GetAnimation("KnockBack_Light");
    private const float damage = 10;
    private const float range = 10;

    protected override AnimationClip AbilityAnimation { get { return attackAnimation; } }
    protected override AnimationClip RecoilAnimation { get { return recoilAnimation; } }
    protected override ScheduledAction[] OtherScheduledActions { get { return null; } }
    protected override float Damage { get { return damage; } }
    public override float Range { get { throw new System.NotImplementedException(); } }

    public Punch(Body body) : base(body, 0.2f, 0.8f)
    {    }

    protected override void ApplyEffects(Body hitBody)
    {
        performer.TurnToFace(hitBody.transform.position);
        base.ApplyEffects(hitBody);
    }
}
