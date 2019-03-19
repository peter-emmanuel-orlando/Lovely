/*


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class AttackCombo : Ability
{
    private static readonly ScheduledMessage activeFramesStart = new ScheduledMessage(StdAnimMsg.activeFramesStart, 0.2f);
    private static readonly ScheduledMessage activeFramesEnd = new ScheduledMessage(StdAnimMsg.activeFramesStart, 0.2f);
    private static readonly ScheduledMessage lockInKnockback = new ScheduledMessage(StdAnimMsg.lockInAnimStart, 0f);
    private static readonly ScheduledMessage unlockInKnockback = new ScheduledMessage(StdAnimMsg.lockInAnimEnd, 1f);

    private static readonly ScheduledMessageAnimation punchHighAnimation = new ScheduledMessageAnimation(_AnimationPool.GetAnimation("Punch_High_R"), StdAnimMsg.aborted, activeFramesStart, activeFramesEnd);
    private static readonly ScheduledMessageAnimation punchMidAnimation = new ScheduledMessageAnimation(_AnimationPool.GetAnimation("Punch_Mid_R"), StdAnimMsg.aborted, activeFramesStart, activeFramesEnd);
    private static readonly ScheduledMessageAnimation strikeMidAnimation = new ScheduledMessageAnimation(_AnimationPool.GetAnimation("Strike_Mid_R"), StdAnimMsg.aborted, activeFramesStart, activeFramesEnd);
    private static readonly ScheduledMessageAnimation punchLowAnimation = new ScheduledMessageAnimation(_AnimationPool.GetAnimation("Punch_Low_R"), StdAnimMsg.aborted, activeFramesStart, activeFramesEnd);

    private static readonly ScheduledMessageAnimation knockBackLightAnimation = new ScheduledMessageAnimation(_AnimationPool.GetAnimation("KnockBack_Light"), StdAnimMsg.aborted, lockInKnockback, unlockInKnockback);
    private static readonly ScheduledMessageAnimation knockBackModerateAnimation = new ScheduledMessageAnimation(_AnimationPool.GetAnimation("KnockBack_Moderate"), StdAnimMsg.aborted, lockInKnockback, unlockInKnockback);
    private static readonly ScheduledMessageAnimation knockBackHeavyAnimation = new ScheduledMessageAnimation(_AnimationPool.GetAnimation("KnockBack_Heavy"), StdAnimMsg.aborted, lockInKnockback, unlockInKnockback);
    
    private static readonly AttackInfo[] combo1 = new AttackInfo[]
    {
        new AttackInfo(strikeMidAnimation, false, HitBoxType.HandR, knockBackModerateAnimation, -5f),
        new AttackInfo(strikeMidAnimation, false, HitBoxType.HandR, knockBackLightAnimation, -4f),
        new AttackInfo(strikeMidAnimation, true, HitBoxType.HandL, knockBackHeavyAnimation, -6f),
    };
    private const float lockPerformFor = 0.2f;
    private const float comboHoldFor = 0.6f;



    private IEnumerator comboEnumerator;
    private readonly HashSet<Body> enemiesAffected = new HashSet<Body>();
    private bool isActive = false;// { get { return statusEnumerator != null && statusEnumerator.Current != null && (statusEnumerator.Current.status == ProgressStatus.InProgress || statusEnumerator.Current.status == ProgressStatus.Pending); } }
    private float lockPerformUntil = 0;
    private float resetComboIfPerformedAfter = 0;
    private ComboTracker currentPlaceInCombo = new ComboTracker();

    public override float Range
    {
        get
        {
            return 0;//currentPlaceInCombo.GetCurrent().punchAnimation. 
        }
    }


    public AttackCombo(UpdateSubscriber SubscribeForUpdate, TriggerEventSubscriber SubscribeForTriggerEvents, UnifiedController inteControl) : base(SubscribeForUpdate, SubscribeForTriggerEvents, inteControl)
    { }

    protected override void Update()
    {
        if (comboEnumerator != null && !comboEnumerator.MoveNext())
            comboEnumerator = null;
    }


    public override ProgressStatus CheckStatus()
    {
        var result = ProgressStatus.Complete;
        if (comboEnumerator != null)
        {

        }
        return result;

    }


    public override IEnumerator<ProgressStatus> CastAbility()
    {
        var result = _CastAbility();
        result.MoveNext();
        return result;
    }

    private IEnumerator<ProgressStatus> _CastAbility()
    {
        //need a way to say which combo to perform
        currentPlaceInCombo.SetCombo(combo1);

        if (Time.time > lockPerformUntil && !isActive)
        {
            if (!currentPlaceInCombo.IsEmpty)
            {
                uniControl.SetHitBoxActiveState(false);
                isActive = false;
                if (Time.time > resetComboIfPerformedAfter || currentPlaceInCombo.IsAtEnd)
                    currentPlaceInCombo.Reset();
                
                var currentMarker = currentPlaceInCombo.GetCurrent();
                var animEnumerator = uniControl.PlayAnimation(currentMarker.attackAnimation.animation, true, currentMarker.isMirrored);
                lockPerformUntil = Time.time + lockPerformFor;
                resetComboIfPerformedAfter = Time.time + currentMarker.attackAnimation.animation.length + comboHoldFor;
                var scheduledMessages = new Queue<ScheduledMessage>(currentMarker.attackAnimation.scheduledMessages);
                while (animEnumerator.MoveNext())
                {
                    //if current time is after active start time, set isactive to true
                    //if current time is after active end time, set isactive to false
                    //make sure active frame triggers at least once

                    var animTime = animEnumerator.Current;
                    while(scheduledMessages.Count > 0 && animTime >= scheduledMessages.Peek().triggerTimeNormalized)
                    {
                        ProcessMessage(scheduledMessages.Dequeue().message);
                    }
                    yield return ProgressStatus.InProgress;
                }
                uniControl.SetHitBoxActiveState(false);
                isActive = false;
            }
        }
    }

    private void ProcessMessage(string message)
    {
        if (message == StdAnimMsg.activeFramesStart)
        {
            var current = currentPlaceInCombo.GetCurrent();
            var handCode = current.hitBoxType;
            var handCollider = uniControl.SetHitBoxActiveState(handCode, true);

            //for debug
            if (handCollider != null)
            {
                var tmp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                tmp.transform.SetParent(handCollider.transform);
                tmp.transform.localPosition = Vector3.zero;
                GameObject.Destroy(tmp, 0.1f);
            }
            isActive = true;
        }
        if (message == StdAnimMsg.activeFramesEnd)
        {
            if (!currentPlaceInCombo.IsAtEnd) currentPlaceInCombo.AdvanceCombo();
            uniControl.SetHitBoxActiveState(false);
            enemiesAffected.Clear();
            isActive = false;
        }
    }
    

    protected override void ReceiveTriggerEvents(Collider collider)
    {
        if (collider.gameObject.layer == LayerMask.NameToLayer("HurtBox") && isActive)
        {

            //Debug.Log("I [" + uniControl + "] punched: " + collider.attachedRigidbody.gameObject);
            var body = collider.GetComponentInParent<Body>();
            if (body != null && !enemiesAffected.Contains(body))
            {
                enemiesAffected.Add(body);
                uniControl.transform.LookAt(body.transform);
                body.transform.LookAt(uniControl.transform);
                var current = currentPlaceInCombo.GetCurrent();
                body.ApplyAbilityEffects(body.Mind, current.deltaHealth, current.knockBackAnimation.animation);
            }
        }
    }
}




*/