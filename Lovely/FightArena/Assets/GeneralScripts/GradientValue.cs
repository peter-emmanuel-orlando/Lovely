using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GradientValue
{
    public readonly float min;
    public readonly float max;

    public GradientValue(float min, float max)
    {
        this.min = min;
        this.max = max;
    }

    public float Lerp(float t)
    {
        return Mathf.Lerp(min, max, t);
    }

    public float LerpUnclamped(float t)
    {
        return Mathf.LerpUnclamped(min, max, t);
    }
}
