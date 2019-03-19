using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashPunch : PhysicalAttack
{
    private static readonly AnimationClip attackAnimation = _AnimationPool.GetAnimation("Strike_Mid_R");
    private static readonly AnimationClip recoilAnimation = _AnimationPool.GetAnimation("KnockBack_Heavy");
    private readonly ScheduledActionQueue scheduledActions = new ScheduledActionQueue();

    protected override AnimationClip AttackAnimation { get { return attackAnimation; } }
    protected override AnimationClip RecoilAnimation { get { return recoilAnimation; } }
    protected override ScheduledActionQueue ScheduledActions { get { return scheduledActions; } }

    public DashPunch(Body body) : base(body)
    {
        scheduledActions = new ScheduledActionQueue
        (
            new ScheduledAction(0f, body.ConsumeEmpowerment),
            new ScheduledAction(0.2f, () => { body.SetHitBoxActiveState(HitBoxType.HandR, true); }),
            new ScheduledAction(0.8f, () => { body.SetHitBoxActiveState(HitBoxType.HandR, false); })
        );
    }
}

public class Punch : PhysicalAttack
{
    private static readonly AnimationClip attackAnimation = _AnimationPool.GetAnimation("Punch_Mid_R");
    private static readonly AnimationClip recoilAnimation = _AnimationPool.GetAnimation("KnockBack_Light");
    private readonly ScheduledActionQueue scheduledActions = new ScheduledActionQueue();

    protected override AnimationClip AttackAnimation { get { return attackAnimation; } }
    protected override AnimationClip RecoilAnimation { get { return recoilAnimation; } }
    protected override ScheduledActionQueue ScheduledActions { get { return scheduledActions; } }

    public Punch(Body body) : base(body)
    {
        scheduledActions = new ScheduledActionQueue
        (
            new ScheduledAction(0f, body.ConsumeEmpowerment),
            new ScheduledAction(0.2f, () => { body.SetHitBoxActiveState(HitBoxType.HandR, true); }),
            new ScheduledAction(0.8f, () => { body.SetHitBoxActiveState(HitBoxType.HandR, false); })
        );
    }
}
