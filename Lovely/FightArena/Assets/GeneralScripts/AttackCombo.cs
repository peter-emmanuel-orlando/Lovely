/*
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ComboAbility : Ability
{
    List<AnimationClip> attacks = new List<AnimationClip>() { _AnimationPool.GetAnimation("KneeThrust"), _AnimationPool.GetAnimation("strike"), _AnimationPool.GetAnimation("strike") };
    List<AnimationClip> recoil = new List<AnimationClip>() { _AnimationPool.GetAnimation("KnockBack_Light"), _AnimationPool.GetAnimation("KnockBack_Moderate"), _AnimationPool.GetAnimation("KnockBack_Heavy") };
    int currentIndex = -1;
    float resetAfter = 0;
    float comboHoldDuration = 1.2f;
    bool isActive = false;
    IEnumerator<ProgressStatus> attackEnumerator;
    HashSet<Body> alreadyAffected = new HashSet<Body>();

    public ComboAbility(UpdateSubscriber SubscribeForUpdate, TriggerEventSubscriber SubscribeForTriggerEvents, Body body) : base(SubscribeForUpdate, SubscribeForTriggerEvents, body)
    {
    }

    public override float Range//of next attack
    {
        get
        {
            var nextIndex = currentIndex + 1;
            nextIndex %= attacks.Count;
            return attacks[nextIndex].apparentSpeed * attacks[nextIndex].averageDuration;
        }
    }

    protected override void Update()
    {
        if (attackEnumerator == null && Time.time > resetAfter) currentIndex = -1;
        if (attackEnumerator != null && !attackEnumerator.MoveNext())
            attackEnumerator = null;
    }

    public override IEnumerator<ProgressStatus> CastAbility()
    {
        if (attackEnumerator == null)
        {
            var result = CastAbilityCoroutine();
            attackEnumerator = result;
            return result;
        }
        else
        {
            return null;
        }
    }

    public IEnumerator<ProgressStatus> CastAbilityCoroutine()
    {
        currentIndex++;
        currentIndex %= attacks.Count;
        resetAfter = Time.time + attacks[currentIndex].averageDuration + comboHoldDuration;
        alreadyAffected.Clear();
        var animEnumerator = body.PlayAnimation(attacks[currentIndex]);
        while (animEnumerator != null && animEnumerator.MoveNext())
        {
            if (animEnumerator.Current > 0.8)
            {
                isActive = true;
                body.SetHitBoxActiveState(true);
            }
            yield return ProgressStatus.InProgress;
            body.gameObject.DisplayTextComponent("Animation: " + attacks[currentIndex] + "Progress: " + attackEnumerator.Current);
        }
        isActive = false;
        body.SetHitBoxActiveState(false);
        alreadyAffected.Clear();
        yield return ProgressStatus.Complete;
    }

    public override ProgressStatus CheckStatus()
    {
        if (attackEnumerator != null)
            return attackEnumerator.Current;
        else
            return ProgressStatus.Complete;
    }

    protected override void ReceiveTriggerEvents(Collider collider)
    {
        var hitBody = collider.gameObject.GetComponentInParent<Body>();
        if (isActive && hitBody != null && hitBody != body && !alreadyAffected.Contains(hitBody) && collider.gameObject.layer == LayerMask.NameToLayer("HurtBox"))
        {
            alreadyAffected.Add(hitBody);
            body.TurnToFace(hitBody.transform.position);
            hitBody.ApplyAbilityEffects(base.body.Mind, -10, recoil[1]);
            Debug.Log("I [" + body.gameObject +"] punched [" + hitBody.gameObject +"]");
        }
    }
}


*/