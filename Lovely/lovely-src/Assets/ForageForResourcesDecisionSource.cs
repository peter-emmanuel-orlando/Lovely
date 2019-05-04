using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Body))]
public class ForageForResourcesDecisionSource : MonoBehaviour, IDecisionSource
{
    ForageForResourcesPerformable forage;


    public IPerformable GetDecisions()
    {
        var stone = TrackedComponent.GetOverlapping<IItemsProvider<IResource>>(new Bounds(Stone.stone.Bounds.center, Stone.stone.Bounds.extents), true);
        foreach (var item in stone)
        {
            var v = item;
            var r = v.ToString();
        }
        return new EmptyPerformable();//forage;
    }

    private void Start()
    {
        var mind = gameObject.GetComponent<Body>().Mind;
        forage = new ForageForResourcesPerformable(mind);
        mind.OverrideDecisionSource(this);
    }
}
/*
[RequireComponent(typeof(Body))]
public class ForageForResourcesDecisionSource : MonoBehaviour, IDecisionSource
{
    ForageForResourcesPerformable forage;

    public IPerformable GetDecisions()
    {
        return forage;
    }

    private void Start()
    {
        var mind = gameObject.GetComponent<Body>().Mind;
        forage = new ForageForResourcesPerformable(mind);
        mind.OverrideDecisionSource(this);
    }
}

[RequireComponent(typeof(Body))]
public class DecisionSource<T> : MonoBehaviour, IDecisionSource where T : IPerformable
{
    T decision;

    public IPerformable GetDecisions()
    {
        return decision;
    }

    private void Start()
    {
        var mind = gameObject.GetComponent<Body>().Mind;
        decision = new T(mind);
        mind.OverrideDecisionSource(this);
    }
}*/
