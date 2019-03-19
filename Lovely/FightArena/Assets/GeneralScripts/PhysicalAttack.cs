using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalAttack : Ability
{
    AnimationClip attackAnimation = _AnimationPool.GetAnimation("Strike_Mid_R");
    AnimationClip recoilAnimation = _AnimationPool.GetAnimation("KnockBack_Heavy");

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


    struct ScheduledAction
    {
        public readonly float time;
        public readonly Guarentees guarentees;
        private readonly Action action;

        public ScheduledAction(float time, Action action) : this(time, Guarentees.None, action)
        {        }

        public ScheduledAction(float time, Guarentees options, Action action)
        {
            if (action == null) throw new ArgumentNullException();
            this.time = time;
            this.guarentees = options;
            this.action = action;
        }

        public void PerformAction()
        {
            action();
        }

        public enum Guarentees
        {
            None = 0,
            GuarenteeExecution,
            GuarenteeFrame
        }
    }

    public override IEnumerator<ProgressStatus> CastAbility()//bool guarenteeEvents = false) // guarentee results makes sure events last at least one frame
    {
        IEnumerator<ProgressStatus> result = null;
        if(innerEnumerator == null && !body.IsLocked)
        {
            result = CastAbilityEnumerator();
            result.MoveNext();
            innerEnumerator = result;
        }
        return result;
    }
    /*
    private IEnumerator<ProgressStatus> CastAbilityEnumerator()
    {
        var checkStatus = body.PlayAnimation(attackAnimation, true, false);
        alreadyHit.Clear();
        var progress = 0f;
        var sa = new Queue<ScheduledAction>();
        sa.Enqueue(new ScheduledAction(0.2f, () => { body.SetHitBoxActiveState(HitBoxType.HandR, true); }));
        sa.Enqueue(new ScheduledAction(0.8f, () => { body.SetHitBoxActiveState(HitBoxType.HandR, false); }));

        var lastUpdated = -1;
        while (progress  != -1 && progress < 1)
        {
            progress = checkStatus.GetProgress();
            if(Time.frameCount > lastUpdated )
            {
                lastUpdated = Time.frameCount;
                while (sa.Count > 0 && progress > sa.Peek().time)
                {
                    var toExecute = sa.Dequeue();
                    toExecute.PerformAction();
                    if (toExecute.guarentees == ScheduledAction.Guarentees.GuarenteeFrame)
                        break;
                }
            }
            yield return ProgressStatus.InProgress;
        }

        while (sa.Count > 0)
        {
            if (Time.frameCount > lastUpdated)
            {
                lastUpdated = Time.frameCount;
                var toExecute = sa.Dequeue();
                if (toExecute.guarentees == ScheduledAction.Guarentees.GuarenteeExecution)
                {
                    toExecute.PerformAction();
                }
                if (toExecute.guarentees == ScheduledAction.Guarentees.GuarenteeFrame)
                {
                    toExecute.PerformAction();
                    yield return ProgressStatus.InProgress;
                }
            }
        }
        
        yield return ProgressStatus.Complete;
        yield break;
    }
    */

        //still affected by how many times called on same frame
    private IEnumerator<ProgressStatus> CastAbilityEnumerator()
    {
        var checkStatus = body.PlayAnimation(attackAnimation, true, false);
        alreadyHit.Clear();
        var progress = 0f;
        var sa = new Queue<ScheduledAction>();
        sa.Enqueue(new ScheduledAction(0.2f, () => { body.SetHitBoxActiveState(HitBoxType.HandR, true); }));
        sa.Enqueue(new ScheduledAction(0.8f, () => { body.SetHitBoxActiveState(HitBoxType.HandR, false); }));
        var lastPerformed = -1;
        while (progress != -1 && progress < 1)
        {
            progress = checkStatus.GetProgress();
            if (lastPerformed != Time.frameCount && sa.Count > 0 && progress > sa.Peek().time)
            {
                lastPerformed = Time.frameCount;
                sa.Dequeue().PerformAction();
            }
            yield return ProgressStatus.InProgress;
        }
        while(sa.Count > 0)
        {
            sa.Dequeue().PerformAction();
        }

        yield return ProgressStatus.Complete;
        yield break;
    }


    public override ProgressStatus CheckStatus()
    {
        throw new System.NotImplementedException();
    }

    private readonly HashSet<Body> alreadyHit = new HashSet<Body>();
    private void ReceiveOnTriggerEnter(object sender, TriggerEventArgs tArgs)
    {
        if(tArgs.collider.gameObject.layer == LayerMask.NameToLayer("HurtBox"))
        {
            var hitBody = tArgs.collider.GetComponentInParent<Body>();
            if (hitBody != null && !alreadyHit.Contains(hitBody) && hitBody != body )
            {
                alreadyHit.Add(hitBody);
                hitBody.TurnToFace(body.transform.position);
                hitBody.ApplyAbilityEffects(body.Mind, -27, recoilAnimation);
            }
        }
    }

    private void ReceiveUpdate(object sender, EventArgs e)
    {
        if (innerEnumerator != null && !innerEnumerator.MoveNext())
            innerEnumerator = null;
    }
}
