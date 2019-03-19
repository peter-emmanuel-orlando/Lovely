using System;
using UnityEngine;
public delegate void UpdateSubscriber(Action updateMethod);
public delegate void AnimationEventSubscriber(Action<string> receiveAnimationEventMethod);
public delegate void TriggerEventSubscriber(Action<Collider> receiveTriggerEventMethod);