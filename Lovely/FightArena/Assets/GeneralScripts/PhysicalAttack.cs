using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class PhysicalAttack : AnimatedAbility
{
    private readonly CollisionTracker collisionTracker;
    private readonly ScheduledAction[] startAndStop;

    protected abstract float Damage { get; }
    protected abstract AnimationClip RecoilAnimation { get; }
    protected abstract ScheduledAction[] OtherScheduledActions { get; }
    protected override sealed ScheduledActionQueue ScheduledActions { get { return new ScheduledActionQueue(OtherScheduledActions, startAndStop); } }

    public PhysicalAttack(Body body, float activeFrameStartNormalized, float activeFrameEndNormalized) : base(body)
    {
        collisionTracker = new CollisionTracker(body);
        startAndStop = new ScheduledAction[]
        {
            new ScheduledAction(activeFrameStartNormalized, () => { SetEnableCollisions(true); }),
            new ScheduledAction(activeFrameEndNormalized, () => { SetEnableCollisions(false); })
        };
    }

    private void SetEnableCollisions(bool enableCollisions)
    {
        if (enableCollisions)
        {
            performer.OnTriggerEnterEvent += OnHit;
            performer.SetHitBoxActiveState(HitBoxType.HandR, true);
        }
        else
        {
            performer.OnTriggerEnterEvent -= OnHit;
            performer.SetHitBoxActiveState(HitBoxType.HandR, false);
            collisionTracker.Reset();
        }
    }

    private void OnHit(GameObject sender, ColliderEventArgs e)
    {
        var hitBody = e.collider.GetComponentInParent<Body>();
        if (hitBody != null && hitBody != performer && !collisionTracker.HasBeenAffected(hitBody))
        {
            ApplyEffects(hitBody);
            collisionTracker.MarkAsAffected(hitBody);
        }
    }

    protected virtual void ApplyEffects(Body hitBody)
    {
        performer.TurnToFace(hitBody.transform.position);
        hitBody.ApplyAbilityEffects(performer.Mind, Damage, RecoilAnimation);
    }
}

/*
public abstract class PhysicalAttack : Ability
{
    protected abstract AnimationClip AttackAnimation { get; }
    protected abstract AnimationClip RecoilAnimation { get; }
    protected abstract ScheduledActionQueue OtherScheduledActions { get; }

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
        if (innerEnumerator == null && !invoker.IsLocked)
        {
            innerEnumerator = CastAbilityEnumerator();
            innerEnumerator.MoveNext();
        }
    }

    private IEnumerator<ProgressStatus> CastAbilityEnumerator()
    {
        var checkStatus = invoker.PlayAnimation(AttackAnimation, true, false);
        alreadyHit.Clear();
        var sa = OtherScheduledActions.GetCopyOfQueue();

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

    private void ReceiveOnTriggerEnter(object sender, ColliderEventArgs tArgs)
    {
        if (tArgs.collider.gameObject.layer == LayerMask.NameToLayer("HurtBox"))
        {
            var hitBody = tArgs.collider.GetComponentInParent<Body>();
            if (hitBody != null && !alreadyHit.Contains(hitBody) && hitBody != invoker)
            {
                alreadyHit.Add(hitBody);
                hitBody.TurnToFace(invoker.transform.position);
                hitBody.ApplyAbilityEffects(invoker.Mind, -27, RecoilAnimation);
            }
        }
    }

    private void ReceiveUpdate(object sender, EventArgs e)
    {
        if (innerEnumerator != null && !innerEnumerator.MoveNext())
            innerEnumerator = null;
    }
}


/*
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class AnimatedAttack : Ability
{
    protected abstract AnimationClip AttackAnimation { get; }
    protected abstract ScheduledActionQueue ScheduledActions { get; }
    IEnumerator<ProgressStatus> innerEnumerator;

    public AnimatedAttack(Body body) : base(body)
    {
        body.UpdateEvent += ReceiveUpdate;
    }


    private void ReceiveUpdate(object sender, EventArgs e)
    {
        if (innerEnumerator != null && !innerEnumerator.MoveNext())
            innerEnumerator = null;
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
        var sa = ScheduledActions.GetCopyOfQueue();

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

    

}

public abstract class PhysicalAttack : AnimatedAttack
{
    protected abstract AnimationClip RecoilAnimation { get; }
    protected abstract float Damage { get; }
    protected abstract bool applyEffectsEachFrame { get; }
    protected abstract bool affectSelf { get; }

    private readonly HashSet<Body> alreadyHit = new HashSet<Body>();
    private readonly HashSet<Body> alreadyHitThisFrame = new HashSet<Body>();

    //dont forget to implement a way to receive hits!
    public PhysicalAttack(Body body) : base(body)
    { }

    protected abstract Collider GetBodiesHitThisFrame

    protected void ReceiveHit(Collider collider)
    {
        if (collider.gameObject.layer == LayerMask.NameToLayer("HurtBox"))
        {
            var hitBody = collider.GetComponentInParent<Body>();
            ReceiveHit(hitBody);
        }
    }
    protected void ReceiveHit(Body hitBody)
    {
        if (hitBody != null && (affectSelf || hitBody != body))
        {
            if (applyEffectsEachFrame && !alreadyHitThisFrame.Contains(hitBody))
            {
                alreadyHitThisFrame.Add(hitBody);
                ApplyEffects(hitBody);
                body.StartCoroutine(ClearEnemiesThisFrame());
            }

            if (!applyEffectsEachFrame && !alreadyHit.Contains(hitBody))
            {
                alreadyHit.Add(hitBody);
                ApplyEffects(hitBody);
            }
        }
    }

    protected virtual void ApplyEffects(Body hitBody)
    {
        hitBody.TurnToFace(body.transform.position);
        hitBody.ApplyAbilityEffects(body.Mind, -Mathf.Abs(Damage), RecoilAnimation);
    }

    private IEnumerator ClearEnemiesThisFrame()
    {
        yield return new WaitForEndOfFrame();
        alreadyHitThisFrame.Clear();
    }
}
*/

