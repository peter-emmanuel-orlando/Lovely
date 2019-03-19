using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionTracker
{
    private readonly HashSet<Body> alreadyAffected = new HashSet<Body>();
    private readonly HashSet<Body> alreadyAffectedThisFrame = new HashSet<Body>();
    private MonoBehaviour coroutineSource;

    public CollisionTracker(MonoBehaviour coroutineSource)
    {
        ChangeCoroutineSource(coroutineSource);
    }

    public void ChangeCoroutineSource( MonoBehaviour coroutineSource)
    {
        this.coroutineSource = coroutineSource;
        CheckForNullCoroutineSource();
    }

    public void MarkAsAffected(Body affectedBody)
    {
        CheckForNullCoroutineSource();

        if (!alreadyAffected.Contains(affectedBody))
            alreadyAffected.Add(affectedBody);
        if(!alreadyAffectedThisFrame.Contains(affectedBody))
        {
            alreadyAffectedThisFrame.Add(affectedBody);
            coroutineSource.StartCoroutine(ClearAlreadyAffectedThisFrame());
        }
    }

    public bool HasBeenAffected(Body affectedBody)
    {
        var result = alreadyAffected.Contains(affectedBody);
        return result;
    }

    public bool HasBeenAffectedThisFrame(Body affectedBody)
    {
        var result = alreadyAffectedThisFrame.Contains(affectedBody);
        return result;
    }

    public void Reset()
    {
        CheckForNullCoroutineSource();
        alreadyAffected.Clear();
        alreadyAffectedThisFrame.Clear();
    }

    private void CheckForNullCoroutineSource()
    {
        if (coroutineSource == null) throw new System.ArgumentNullException("A gameobject is needed to start this objects coroutines");
    }

    private IEnumerator ClearAlreadyAffectedThisFrame()
    {
        yield return new WaitForEndOfFrame();
        alreadyAffectedThisFrame.Clear();
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

