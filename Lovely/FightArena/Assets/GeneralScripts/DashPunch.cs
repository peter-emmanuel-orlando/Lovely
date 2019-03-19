using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashPunch : PhysicalAttack
{
    private static readonly AnimationClip attackAnimation = _AnimationPool.GetAnimation("Strike_Mid_R");
    private static readonly AnimationClip recoilAnimation = _AnimationPool.GetAnimation("KnockBack_Heavy");
    private readonly ScheduledAction[] otherScheduledActions;
    private const float damage = 10;
    private const float range = 10;

    protected override AnimationClip AbilityAnimation { get { return attackAnimation; } }
    protected override AnimationClip RecoilAnimation { get { return recoilAnimation; } }
    protected override ScheduledAction[] OtherScheduledActions { get { return otherScheduledActions; } }
    protected override float Damage { get { return damage; } }
    public override float Range { get { throw new System.NotImplementedException(); } }

    public DashPunch(Body body) : base(body, 0.2f, 0.8f)
    {
        otherScheduledActions = new ScheduledAction[] { new ScheduledAction(0f, body.ConsumeEmpowerment) };
    }
}
