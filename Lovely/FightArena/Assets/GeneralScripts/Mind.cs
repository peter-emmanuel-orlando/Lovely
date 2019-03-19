using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Mind : IDecisionMaker
{
    Body body;
    public Body Body { get { return body; } }
    IDecisionMaker decisionSource;
    IPerformable currentPerformable;
    IEnumerator currentEnumerator;

    public Mind(Body body)
    {
        this.body = body;
        body.SubscribeForUpdates(Update);
    }

    public abstract IPerformable GetDecisions();

    public void OverrideDecisionMaker(IDecisionMaker newDecisionSource)
    {
        decisionSource = newDecisionSource;
    }
    public IDecisionMaker GetCurrentDecisionMaker()
    {
        return decisionSource;
    }

    void Update()
    {
        ManagePerformable();
    }

    void ManagePerformable()
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
}
