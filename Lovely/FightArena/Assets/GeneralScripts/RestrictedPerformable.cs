using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RestrictedPerformable<T> : Performable where T : IBodyFeature
{
    public readonly  T extraFeature;
    public readonly Body body;

    public RestrictedPerformable(Body body, T extraFeature)
    {
        this.body = body;
        this.extraFeature = extraFeature;
    }
}
