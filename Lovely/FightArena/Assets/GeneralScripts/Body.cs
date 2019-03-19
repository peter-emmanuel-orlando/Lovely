using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// bodies are the physical representation of creatures. As such Body : UnifiedController because bodies can move and animate.
/// The diference is that every Body has a Mind which controlls it. Body regulates bodyStats like hunger, damage, stamina.
/// Body has a list of abilities associated with that body, but mind is the portion that can activate the abilities, triggering them in body.
/// think of body as a puppet that can be manipulated by either a performable or an ability. DecisionMakers are basically coreographers who 
/// organize all the performables into a logical dance.
/// so abilities should be triggered by performables; performables should be chosen by decision makers; the performables chosen by decision 
/// makers should then manipulate body. Though it is possible to just call the method to activate a performable or an ability in itself, this should not be done.
/// the same goes for moving body manually. If manual control is desired, body should be eschewed in favor of unified controller
/// 
/// body loads the prefab of its specific model and avatar and override controller
/// body also create a new mind of the appropriate type
/// </summary>
public class Body : UnifiedController
{
    private Mind mind;
    public Mind Mind { get { return mind; } }

    protected override void Awake()
    {
        base.Awake();
        mind = new EmptyMind(this);
    }

    private void _Update()
    {

    }

    private void _ReceiveAnimationEvents(string message)
    {

    }

    private void _OnTriggerStay(Collider collider)
    {

    }





    //************************************************************
    //********for messages and callbacks**************************

    private Action updateCallbacks = delegate() { };
    private Action<string> animationEventsCallbacks = delegate(string s) { };
    private Action<Collider> triggerEventsCallbacks = delegate(Collider col) { };
    public void SubscribeForUpdates(Action updateMethod)
    {
        updateCallbacks += updateMethod;
    }
    public void SubscribeForAnimationEvents(Action<string> receiveAnimationEventsMethod)
    {
        animationEventsCallbacks += receiveAnimationEventsMethod;
    }
    public void SubscribeForTriggerEvents(Action<Collider> receiveTriggerEventsMethod)
    {
        triggerEventsCallbacks += receiveTriggerEventsMethod;
    }
    //*********************************************
    protected override void Update()
    {
        base.Update();
        _Update();
        updateCallbacks();
    }
    protected override void ReceiveAnimationEvents(string message)
    {
        base.ReceiveAnimationEvents(message);
        _ReceiveAnimationEvents(message);
        animationEventsCallbacks(message);
    }
    protected virtual void OnTriggerStay(Collider collider)
    {
        _OnTriggerStay(collider);
        triggerEventsCallbacks(collider);
    }
}
