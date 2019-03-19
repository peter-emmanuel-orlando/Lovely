using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Body))]
public class _WanderDecisionMaker : MonoBehaviour, IDecisionMaker
{
    WanderPerformable wander;

    public IPerformable GetDecisions()
    {
        return wander;
    }

    private void Start()
    {
        var mind = gameObject.GetComponent<Body>().Mind;
        mind.OverrideDecisionMaker(this);
        wander = new WanderPerformable(mind);
    }
    
}
