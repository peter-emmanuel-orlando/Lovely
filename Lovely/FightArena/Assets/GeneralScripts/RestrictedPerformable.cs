using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RestrictedPerformable<T> : IPerformable where T : IBodyFeature
{
    public readonly  T extraFeature;
    public readonly Body body;

    public RestrictedPerformable(Body body, T extraFeature)
    {
        this.body = body;
        this.extraFeature = extraFeature;
    }

    public Mind Performer { get { return body.Mind; } }

    public abstract IEnumerator Perform();
}
