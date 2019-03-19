using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Mind : IDecisionMaker
{

    //properties
    private Body body;
    private IDecisionMaker decisionSource;
    //private Dictionary<IPerformable, IEnumerator> currentPerformables;
    private IPerformable currentPerformable;
    private IEnumerator currentEnumerator;
    private readonly List<Intel> visibleEnemies = new List<Intel>();
    private GradientValue perceptionDelayGradient = new GradientValue(0.2f, 5f);
    private float currentAlertness = 0;
    private float lookAroundAfter = 0;


    //getters/setters
    public Body Body { get { return body; } }
    protected abstract float SightRange { get; }
    protected float SightRadius { get { return SightRange * 0.75f; } }
    public List<Intel> VisibleEnemies { get { return new List<Intel>(visibleEnemies); } }
    private float PerceptionDelay { get { return perceptionDelayGradient.Lerp(currentAlertness); } }


    //constuctors
    public Mind(Body body)
    {
        this.body = body;
        body.UpdateEvent += ReceiveUpdates;
    }


    //private methods
    private void ReceiveUpdates(object senter, EventArgs e)
    {
        ManagePerformable();
        ManagePerception();
    }

    private void ManagePerformable()
    {
        IPerformable newDecision;

        if (decisionSource == null)
            newDecision = GetDecisions();
        else
            newDecision = decisionSource.GetDecisions();

        if(newDecision != currentPerformable)
        {
            currentPerformable = newDecision;
            currentEnumerator = newDecision.Perform();
        }

        if(currentEnumerator != null && !currentEnumerator.MoveNext())
        {
            currentPerformable = null;
            currentEnumerator = null;
        }
    }

    private void ManagePerception()
    {
        if(Time.time > lookAroundAfter)
        {
            lookAroundAfter = Time.time + PerceptionDelay;
            LookAround();
        }
    }

    private void LookAround()
    {
        visibleEnemies.Clear();
        var inRange = Physics.OverlapSphere(body.CameraBone.position, SightRange);//Physics.OverlapCapsule(body.CameraBone.position, body.CameraBone.forward * SightRange, SightRadius, Physics.AllLayers, QueryTriggerInteraction.Collide);
        foreach (var col in inRange)
        {
            var current = col.GetComponentInParent<ISpawnable>();
            if(current != null && !object.ReferenceEquals(current, body))
                visibleEnemies.Add(new Intel(body.gameObject, current));
        }
        visibleEnemies.Distinct();
        visibleEnemies.Sort();
    }

    //public methods
    public abstract IPerformable GetDecisions();

    public void OverrideDecisionMaker(IDecisionMaker newDecisionSource)
    {
        decisionSource = newDecisionSource;
    }

    public IDecisionMaker GetCurrentDecisionMaker()
    {
        return decisionSource;
    }



}
