using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TESTSCRIPT_Punch : Ability
{

    static AnimationEvent activeFramesStart = AnimationEventMessages.GetActiveFramesStartEvent(0);
    static AnimationEvent activeFramesEnd = AnimationEventMessages.GetActiveFramesEndEvent(0);

    static AnimationEvent lockInKnockback = AnimationEventMessages.GetAnimationLockEvent(0);
    static AnimationEvent unlockInKnockback = AnimationEventMessages.GetAnimationUnlockEvent(0);

    static AnimationClip punchAnimation = _AnimationPool.GetAnimation("Punch_Mid_R").AddEventsAtNormalizedTime(new AnimationEvent[] { activeFramesStart, activeFramesEnd }, new float[] {0.2f, 0.9f });
    static AnimationClip knockBackAnimation = _AnimationPool.GetAnimation("KnockBack_Heavy").AddEventsAtNormalizedTime(new AnimationEvent[] { lockInKnockback, unlockInKnockback }, new float[] { 0f, 0.9f });

    bool isActive = false;
    UnifiedController enemyController;

    public TESTSCRIPT_Punch(UpdateSubscriber SubscribeForUpdate, AnimationEventSubscriber SubscribeForAnimationEvents, TriggerEventSubscriber SubscribeForTriggerEvents, UnifiedController inteControl) : base(SubscribeForUpdate, SubscribeForAnimationEvents,  SubscribeForTriggerEvents, inteControl)
    {    }

    public override AbilityStatus CheckStatus()
    {
        throw new NotImplementedException();
    }

    public override void Perform()
    {
        uniControl.PlayAnimation(punchAnimation);
    }

    protected override void ReceiveAnimationEvents(string message)
    {
        Debug.Log(this + "received an animationEvent message: " + message);
        if (message == AnimationEventMessages.activeFramesStart)
        {
            uniControl.SetHitBoxActiveState(UnifiedController.HitBoxType.HandR, true);
            var handR = uniControl.transform.FindDeepChild("hand.R");
            var tmp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tmp.transform.SetParent(handR);
            tmp.transform.localPosition = Vector3.zero;
            GameObject.Destroy(tmp, 0.1f);
            isActive = true;
        }
        if (message == AnimationEventMessages.activeFramesEnd)
        {
            uniControl.SetHitBoxActiveState(UnifiedController.HitBoxType.HandR, false);
            var handR = uniControl.transform.FindDeepChild("hand.R");
            isActive = false;
        }
    }

    protected override void ReceiveTriggerEvents(Collider collider)
    {
        if(collider.gameObject.layer == LayerMask.NameToLayer("HurtBox") && isActive)
        {
            Debug.Log("I [" + uniControl + "] punched: " + collider.attachedRigidbody.gameObject);
            var cont = collider.GetComponentInParent<UnifiedController>();
            if (cont != null)
            {
                uniControl.transform.LookAt(cont.transform);
                cont.transform.LookAt(uniControl.transform);
                cont.PlayAnimation(knockBackAnimation);
            }
        }
    }

    protected override void Update()
    {

    }
}