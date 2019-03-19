using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TESTSCRIPT_Punch : Ability
{
    //possibleMessages sent through the animationEvent system:
    //LockInAnimation:True;
    //LockInAnimation:False;
    //CanMoveInAnimation:True;
    //CanMoveInAnimation:False;
    //WindUp:Start
    //WindUp:End
    //ActiveFrames:Start
    //ActiveFrames:End
    //RecoveryFrames:Start
    //RecoveryFrames:End

    static AnimationEvent activeFramesStart = AnimationEventMessages.GetActiveFramesStartEvent(0.2f);
    static AnimationEvent activeFramesEnd = AnimationEventMessages.GetActiveFramesEndEvent(0.9f);

    static AnimationEvent lockInKnockback = AnimationEventMessages.GetAnimationLockEvent(0);
    static AnimationEvent unlockInKnockback = AnimationEventMessages.GetAnimationUnlockEvent(1);

    static AnimationClip punchAnimation = _AnimationPool.GetAnimation("Punch_Mid_R").AddEventsToAnimation(activeFramesStart, activeFramesEnd);
    static AnimationClip knockBackAnimation = _AnimationPool.GetAnimation("KnockBack_Heavy").AddEventsToAnimation(lockInKnockback, unlockInKnockback);

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
        inteControl.PlayAnimation(punchAnimation);
    }

    protected override void ReceiveAnimationEvents(string message)
    {
        Debug.Log(this + "received an animationEvent message: " + message);
        if (message == AnimationEventMessages.activeFramesStart)
        {
            inteControl.SetHitBoxActiveState(UnifiedController.HitBoxType.HandR, true);
            var handR = inteControl.transform.FindDeepChild("hand.R");
            var tmp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tmp.transform.SetParent(handR);
            tmp.transform.localPosition = Vector3.zero;
            GameObject.Destroy(tmp, 0.1f);
            isActive = true;
        }
        if (message == AnimationEventMessages.activeFramesEnd)
        {
            inteControl.SetHitBoxActiveState(UnifiedController.HitBoxType.HandR, false);
            var handR = inteControl.transform.FindDeepChild("hand.R");
            isActive = false;
        }
    }

    protected override void ReceiveTriggerEvents(Collider collider)
    {
        if(collider.gameObject.layer == LayerMask.NameToLayer("HurtBox") && isActive)
        {
            Debug.Log("I [" + inteControl + "] punched: " + collider.attachedRigidbody.gameObject);
            var cont = collider.GetComponentInParent<Being>();
            if (cont != null)
            {
                inteControl.transform.LookAt(cont.transform);
                cont.transform.LookAt(inteControl.transform);
                cont.PlayAnimation(knockBackAnimation);
            }
        }
    }

    protected override void Update()
    {

    }
}