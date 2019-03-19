using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PhysicalAttack : Ability
{
    protected abstract AnimationClip AttackAnimation { get; }
    protected abstract AnimationClip RecoilAnimation { get; }
    protected abstract ScheduledAction[] ScheduledActions { get; }

    private readonly HashSet<Body> alreadyHit = new HashSet<Body>();
    IEnumerator<ProgressStatus> innerEnumerator;

    public PhysicalAttack(Body body) : base(body)
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

    public override void CastAbility()//bool guarenteeEvents = false) // guarentee results makes sure events last at least one frame
    {
        if (innerEnumerator == null && !body.IsLocked)
        {
            innerEnumerator = CastAbilityEnumerator();
            innerEnumerator.MoveNext();
        }
    }

    private IEnumerator<ProgressStatus> CastAbilityEnumerator()
    {
        var checkStatus = body.PlayAnimation(AttackAnimation, true, false);
        alreadyHit.Clear();
        var sa = new Queue<ScheduledAction>(ScheduledActions);

        var progress = 0f;
        var lastFrame = -1;

        while (progress < 1)
        {
            var newProgress = checkStatus.GetProgress();
            if (newProgress == -1)
                break;
            else
            {
                progress = newProgress;
                if (lastFrame != Time.frameCount && sa.Count > 0 && progress > sa.Peek().time)
                {
                    lastFrame = Time.frameCount;
                    sa.Dequeue().PerformAction();
                }
                yield return ProgressStatus.InProgress;
            }
        }
        while (sa.Count > 0)
        {
            sa.Dequeue().PerformAction();
        }
        //if (progress < 1)
        //    yield return ProgressStatus.Aborted;
        //else
        yield return ProgressStatus.Complete;
        yield break;
    }


    public override ProgressStatus CheckStatus()
    {
        var result = ProgressStatus.Complete;
        if (innerEnumerator != null)
            result = innerEnumerator.Current;
        return result;
    }

    private void ReceiveOnTriggerEnter(object sender, TriggerEventArgs tArgs)
    {
        if (tArgs.collider.gameObject.layer == LayerMask.NameToLayer("HurtBox"))
        {
            var hitBody = tArgs.collider.GetComponentInParent<Body>();
            if (hitBody != null && !alreadyHit.Contains(hitBody) && hitBody != body)
            {
                alreadyHit.Add(hitBody);
                hitBody.TurnToFace(body.transform.position);
                hitBody.ApplyAbilityEffects(body.Mind, -27, RecoilAnimation);
            }
        }
    }

    private void ReceiveUpdate(object sender, EventArgs e)
    {
        if (innerEnumerator != null && !innerEnumerator.MoveNext())
            innerEnumerator = null;
    }
}
