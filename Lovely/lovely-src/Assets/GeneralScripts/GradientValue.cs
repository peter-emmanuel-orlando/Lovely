using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GradientValue
{
    public readonly float value1;
    public readonly float value2;

    public GradientValue(float value1, float value2)
    {
        this.value1 = value1;
        this.value2 = value2;
    }

    public float Lerp(float t)
    {
        return Mathf.Lerp(value1, value2, t);
    }

    public float LerpUnclamped(float t)
    {
        return Mathf.LerpUnclamped(value1, value2, t);
    }
}
