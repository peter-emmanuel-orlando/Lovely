using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalAttack : Ability
{
    AnimationClip animation = _AnimationPool.GetAnimation("Strike_Mid_R");
    float deltaHealth = 5;
    IEnumerator innerEnumerator;

    //string animation name
    public PhysicalAttack(Body body) : base( body)
    {
        body.UpdateEvent += ReceiveUpdate;
        body.OnTriggerEnterEvent += ReceiveOnTriggerEnter;
    }

    public override float Range
    {
        get
        {
            throw new System.NotImplementedException();
        }
    }

    public override IEnumerator<ProgressStatus> CastAbility()
    {
        IEnumerator<ProgressStatus> result = null;
        if(innerEnumerator == null)
        {
            result = CastAbilityEnumerator();
            result.MoveNext();
            innerEnumerator = result;
        }
        return result;
    }

    private IEnumerator<ProgressStatus> CastAbilityEnumerator()
    {
        var enumerator = body.PlayAnimation(animation, true);
        body.SetHitBoxActiveState(HitBoxType.HandR, true);
        while(enumerator != null && enumerator.MoveNext())
        {
            yield return ProgressStatus.InProgress;
        }
        body.SetHitBoxActiveState(HitBoxType.HandR, false);
        yield return ProgressStatus.Complete;
        yield break;
    }

    public override ProgressStatus CheckStatus()
    {
        throw new System.NotImplementedException();
    }

    private void ReceiveOnTriggerEnter(object sender, TriggerEventArgs tArgs)
    {
        throw new System.NotImplementedException();
    }

    private void ReceiveUpdate(object sender, EventArgs e)
    {
        if (innerEnumerator != null && !innerEnumerator.MoveNext())
            innerEnumerator = null;
    }
}
