using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ability
{
    protected readonly UnifiedController inteControl;

    public Ability(UpdateSubscriber SubscribeForUpdate, AnimationEventSubscriber SubscribeForAnimationEvents, TriggerEventSubscriber SubscribeForTriggerEvents, UnifiedController inteControl)
    {
        SubscribeForUpdate(Update);
        SubscribeForAnimationEvents(ReceiveAnimationEvents);
        SubscribeForTriggerEvents(ReceiveTriggerEvents);
        this.inteControl = inteControl;
    }


    public abstract AbilityStatus CheckStatus();

    public abstract void Perform();

    protected abstract void ReceiveAnimationEvents(string message);

    protected abstract void ReceiveTriggerEvents(Collider collider);

    protected abstract void Update();
}

public enum AbilityStatus
{

}