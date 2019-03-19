using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PunchCombo : Ability
{
    private static readonly AnimationEvent activeFramesStart = AnimationEventMessages.GetActiveFramesStartEvent(0);
    private static readonly AnimationEvent activeFramesEnd = AnimationEventMessages.GetActiveFramesEndEvent(0);
    private static readonly AnimationEvent lockInKnockback = AnimationEventMessages.GetAnimationLockEvent(0);
    private static readonly AnimationEvent unlockInKnockback = AnimationEventMessages.GetAnimationUnlockEvent(0);
    private static readonly AnimationClip punchHighAnimation = _AnimationPool.GetAnimation("Punch_High_R").AddEventsAtNormalizedTime(new AnimationEvent[] { activeFramesStart, activeFramesEnd }, new float[] { 0.2f, 0.9f });
    private static readonly AnimationClip punchMidAnimation = _AnimationPool.GetAnimation("Punch_Mid_R").AddEventsAtNormalizedTime(new AnimationEvent[] { activeFramesStart, activeFramesEnd }, new float[] { 0.2f, 0.9f });
    private static readonly AnimationClip punchLowAnimation = _AnimationPool.GetAnimation("Punch_Low_R").AddEventsAtNormalizedTime(new AnimationEvent[] { activeFramesStart, activeFramesEnd }, new float[] { 0.2f, 0.9f });
    private static readonly AnimationClip knockBackLightAnimation = _AnimationPool.GetAnimation("KnockBack_Light").AddEventsAtNormalizedTime(new AnimationEvent[] { lockInKnockback, unlockInKnockback }, new float[] { 0f, 0.9f });
    private static readonly AnimationClip knockBackModerateAnimation = _AnimationPool.GetAnimation("KnockBack_Moderate").AddEventsAtNormalizedTime(new AnimationEvent[] { lockInKnockback, unlockInKnockback }, new float[] { 0f, 0.9f });
    private static readonly AnimationClip knockBackHeavyAnimation = _AnimationPool.GetAnimation("KnockBack_Heavy").AddEventsAtNormalizedTime(new AnimationEvent[] { lockInKnockback, unlockInKnockback }, new float[] { 0f, 0.9f });
    private static readonly PunchStats[] combo1 = new PunchStats[]
    {
        new PunchStats(punchLowAnimation, false, UnifiedController.HitBoxType.HandR, knockBackModerateAnimation, -5f),
        new PunchStats(punchMidAnimation, false, UnifiedController.HitBoxType.HandR, knockBackLightAnimation, -4f),
        new PunchStats(punchHighAnimation, true, UnifiedController.HitBoxType.HandL, knockBackHeavyAnimation, -6f),
    };
    private const float lockPerformFor = 0.2f;
    private const float comboHoldFor = 0.6f;




    private readonly HashSet<Body> enemiesAffected = new HashSet<Body>();
    private bool isActive = false;
    private float lockPerformUntil = 0;
    private float resetComboIfPerformedAfter = 0;
    ComboPlaceMarker currentPlaceInCombo = new ComboPlaceMarker();


    public PunchCombo(UpdateSubscriber SubscribeForUpdate, AnimationEventSubscriber SubscribeForAnimationEvents, TriggerEventSubscriber SubscribeForTriggerEvents, UnifiedController inteControl) : base(SubscribeForUpdate, SubscribeForAnimationEvents, SubscribeForTriggerEvents, inteControl)
    { }

    public override AbilityStatus CheckStatus()
    {
        throw new NotImplementedException();
    }


    public override void Perform()
    {
        //need a way to say which combo to perform
        currentPlaceInCombo.SetCombo(combo1);

        if (Time.time > lockPerformUntil && !isActive)
        {
            if (currentPlaceInCombo.IsEmpty) return;

            if (Time.time > resetComboIfPerformedAfter || currentPlaceInCombo.IsAtEnd)
                currentPlaceInCombo.Reset();

            var current = currentPlaceInCombo.GetCurrent();
            uniControl.PlayAnimation(current.punchAnimation, true, current.isMirrored);
            lockPerformUntil = Time.time + lockPerformFor;
            resetComboIfPerformedAfter = Time.time + current.punchAnimation.length + comboHoldFor;
        }
    }

    protected override void ReceiveAnimationEvents(string message)
    {
        Debug.Log(this + " received an animationEvent message: " + message);
        if (message == AnimationEventMessages.activeFramesStart)
        {
            var current = currentPlaceInCombo.GetCurrent();
            var handCode = current.hitBoxType;
            var handCollider = uniControl.SetHitBoxActiveState(handCode, true);
            if(handCollider != null)
            {
                var tmp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                tmp.transform.SetParent(handCollider.transform);
                tmp.transform.localPosition = Vector3.zero;
                GameObject.Destroy(tmp, 0.1f);
            }
            isActive = true;
        }
        if (message == AnimationEventMessages.activeFramesEnd)
        {
            var current = currentPlaceInCombo.GetCurrent();
            var handCode = current.hitBoxType;
            uniControl.SetHitBoxActiveState(handCode, false);
            isActive = false;
            currentPlaceInCombo.AdvanceCombo();
            enemiesAffected.Clear();
        }
    }

    protected override void ReceiveTriggerEvents(Collider collider)
    {
        if (collider.gameObject.layer == LayerMask.NameToLayer("HurtBox") && isActive)
        {

            Debug.Log("I [" + uniControl + "] punched: " + collider.attachedRigidbody.gameObject);
            var body = collider.GetComponentInParent<Body>();
            if (body != null && !enemiesAffected.Contains(body))
            {
                enemiesAffected.Add(body);
                uniControl.transform.LookAt(body.transform);
                body.transform.LookAt(uniControl.transform);
                var current = currentPlaceInCombo.GetCurrent();
                body.ApplyAbilityEffects(body.Mind, current.deltaHealth, current.knockBackAnimation);
            }
        }
    }

    protected override void Update()
    {

    }
}








