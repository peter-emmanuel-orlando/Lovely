using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StatBlock<StatKey, StatValue>
{
    private readonly Dictionary<StatKey, StatValue> _statDict = new Dictionary<StatKey, StatValue>();

    protected StatBlock(StatValue defaultValue, params StatKey[] statKeys)
    {
        foreach (var key in statKeys)
        {
            _statDict.Add(key, defaultValue);
        }
    }
}

public class NumericalStatBlock<StatKey> : StatBlock<StatKey, float>
{
    public NumericalStatBlock(float defaultValue, params StatKey[] statKeys) : base(defaultValue, statKeys)
    {

    }
}

public class ClampedFloat
{
    private float maxVal;
    private float current;
    private float minVal;
    private bool canOverflow;
    private bool canUnderflow;
    

    public float Current
    {
        get
        {
            return current;
        }

        set
        {
            if (!canOverflow) value = Mathf.Clamp(value, float.NegativeInfinity, maxVal);
            if (!canUnderflow) value = Mathf.Clamp(value, minVal, float.PositiveInfinity);
            current = value;
        }
    }

    public bool CanOverflow
    {
        get
        {
            return canOverflow;
        }

        set
        {
            canOverflow = value;
            if (!canOverflow) current = Mathf.Clamp(current, float.NegativeInfinity, maxVal);
        }
    }

    public bool CanUnderflow
    {
        get
        {
            return canUnderflow;
        }

        set
        {
            canUnderflow = value;
            if (!canUnderflow) current = Mathf.Clamp(current, minVal, float.PositiveInfinity);
        }
    }

    public float MaxVal
    {
        get
        {
            return maxVal;
        }

        set
        {
            maxVal = value;
            if (!canOverflow) current = Mathf.Clamp(current, float.NegativeInfinity, maxVal);
        }
    }

    public float MinVal
    {
        get
        {
            return minVal;
        }

        set
        {
            minVal = value;
            if (!canUnderflow) current = Mathf.Clamp(current, minVal, float.PositiveInfinity);
        }
    }

    //implicit convert to float
    //implement operators through current
    //clampedAssignment
}

public class ClampedStatBlock<StatKey> : NumericalStatBlock<StatKey>
{
    public ClampedStatBlock(float defaultValue, params StatKey[] statKeys) : base(defaultValue, statKeys)
    {
    }
}



//increment all by value
//decriment all by value

//public interface IReadOnlyStat<StatKey, StatValue>
//public interface IReadWriteStat<StatKey, StatValue>