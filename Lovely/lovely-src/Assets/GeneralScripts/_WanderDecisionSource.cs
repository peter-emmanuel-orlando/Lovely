using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Body))]
public class _WanderDecisionSource : MonoBehaviour, IDecisionSource
{
    WanderPerformable wander;

    public IPerformable GetDecisions()
    {
        return wander;
    }

    private void Start()
    {
        var mind = gameObject.GetComponent<Body>().Mind;
        mind.OverrideDecisionSource(this);
        wander = new WanderPerformable(mind);
    }
    
}
