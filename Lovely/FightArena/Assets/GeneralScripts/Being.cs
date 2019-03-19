using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Being : UnifiedController
{
    private Action updateCallbacks;
    private Action<string> animationEventsCallbacks;
    private Action<Collider> triggerEventsCallbacks;
    protected void SubscribeForUpdates(Action updateMethod)
    {
        updateCallbacks += updateMethod;
    }
    protected void SubscribeForAnimationEvents(Action<string> receiveAnimationEventsMethod)
    {
        animationEventsCallbacks += receiveAnimationEventsMethod;
    }
    protected void SubscribeForTriggerEvents(Action<Collider> receiveTriggerEventsMethod)
    {
        triggerEventsCallbacks += receiveTriggerEventsMethod;
    }
    //*********************************************
    protected override void Update()
    {
        base.Update();
        updateCallbacks();
    }
    protected override void ReceiveAnimationEvents(string message)
    {
        base.ReceiveAnimationEvents(message);
        animationEventsCallbacks(message);
    }
    protected virtual void OnTriggerStay(Collider collider)
    {
        triggerEventsCallbacks(collider);
    }

}
