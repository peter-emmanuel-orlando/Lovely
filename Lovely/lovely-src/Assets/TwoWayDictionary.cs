using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// if T1 has a T2, Then T2 has a T1
/// </summary>
/// <typeparam name="T1"></typeparam>
/// <typeparam name="T2"></typeparam>
public class TwoWayDictionary<T1, T2>
{

    private Dictionary<T1, HashSet<T2>> T1ToT2 { get; } = new Dictionary<T1, HashSet<T2>>();
    private Dictionary<T2, HashSet<T1>> T2ToT1 { get; } = new Dictionary<T2, HashSet<T1>>();

    public bool IsSymmetric { get; } = true;
    public bool IsEmpty { get => T1ToT2.Count == 0 && T2ToT1.Count == 0; }

    //**********************************************************
    //**********************************************************
    //**********************************************************
    public IEnumerable<T2> this[T1 key] { get => T1ToT2[key]; }
    public IEnumerable<T1> this[T2 key] { get => T2ToT1[key]; }


    //******************************************
    public IEnumerable<T1> ForwardKeys => T1ToT2.Keys;
    public IEnumerable<T2> ReverseKeys => T2ToT1.Keys;


    //******************************************
    public IEnumerable<T2> ForwardValues
    {
        get
        {
            if (IsSymmetric)
                foreach (var value in T2ToT1.Keys)
                    yield return value;
            else
                foreach (var values in T1ToT2.Values)
                    foreach (var value in values)
                        yield return value;
        }
    }
    public IEnumerable<T1> ReverseValues
    {
        get
        {
            if (IsSymmetric)
                foreach (var value in T1ToT2.Keys)
                    yield return value;
            else
                foreach (var values in T2ToT1.Values)
                    foreach (var value in values)
                        yield return value;
        }
    }


    //******************************************
    public int ForwardCount => ((IDictionary<T1, HashSet<T2>>)T1ToT2).Count;
    public int ReverseCount => ((IDictionary<T2, HashSet<T1>>)T2ToT1).Count;


    //******************************************
    public bool ForwardIsReadOnly => ((IDictionary<T1, HashSet<T2>>)T1ToT2).IsReadOnly;
    public bool ReverseIsReadOnly => ((IDictionary<T2, HashSet<T1>>)T2ToT1).IsReadOnly;

    private int? overrideHashCode { get; } = null;
    public override int GetHashCode()
    {
        return overrideHashCode ?? base.GetHashCode();
    }
    private TwoWayDictionary(int hashCode, bool isSymmetric, Dictionary<T1, HashSet<T2>> t1ToT2, Dictionary<T2, HashSet<T1>> t2ToT1)
    {
        overrideHashCode = hashCode;
        IsSymmetric = isSymmetric;
        T1ToT2 = t1ToT2 ?? throw new ArgumentNullException(nameof(t1ToT2));
        T2ToT1 = t2ToT1 ?? throw new ArgumentNullException(nameof(t2ToT1));
    }

    public TwoWayDictionary(bool isSymmetric)
    {
        IsSymmetric = isSymmetric;
    }

    //******************************************
    public void AddRange(TwoWayDictionary<T1, T2> other)
    {
        foreach (var t1 in other.T1ToT2.Keys)
        {
            foreach (var t2 in other.T1ToT2[t1])
            {
                Add(t1, t2);
            }
        }
        if(!IsSymmetric || !other.IsSymmetric)
        {
            foreach (var t2 in other.T2ToT1.Keys)
            {
                foreach (var t1 in other.T2ToT1[t2])
                {
                    Add(t2, t1);
                }
            }
        }
    }
    public void AddRange(IEnumerable<KeyValuePair<T1, T2>> keyValuePairs)
    {
        foreach (var keyValuePair in keyValuePairs)
        {
            Add(keyValuePair.Key, keyValuePair.Value);
        }
    }
    public void Add(T1 key, T2 value)
    {
        if (!T1ToT2.ContainsKey(key))
            T1ToT2.Add(key, new HashSet<T2>());
        T1ToT2[key].Add(value);

        if(IsSymmetric)
        {
            if (!T2ToT1.ContainsKey(value))
                T2ToT1.Add(value, new HashSet<T1>());
            T2ToT1[value].Add(key);
        }
    }
    public void AddRange(TwoWayDictionary<T2, T1> other)
    {
        AddRange((TwoWayDictionary < T1, T2 >)other);
    }
    public void AddRange(IEnumerable<KeyValuePair<T2, T1>> keyValuePairs)
    {
        foreach (var keyValuePair in keyValuePairs)
        {
            Add(keyValuePair.Key, keyValuePair.Value);
        }
    }
    public void Add(T2 key, T1 value)
    {
        if (!T2ToT1.ContainsKey(key))
            T2ToT1.Add(key, new HashSet<T1>());
        T2ToT1[key].Add(value);

        if (IsSymmetric)
        {
            if (!T1ToT2.ContainsKey(value))
                T1ToT2.Add(value, new HashSet<T2>());
            T1ToT2[value].Add(key);
        }
    }


    //******************************************
    public bool ContainsKey(T1 key)
    {
        return T1ToT2.ContainsKey(key);
    }
    public bool ContainsKey(T2 key)
    {
        return T2ToT1.ContainsKey(key);
    }

    //******************************************
    public bool Remove(T1 key)
    {
        var success = T1ToT2.ContainsKey(key);
        if (success)
        {
            if (IsSymmetric)
            {
                foreach (var T2Key in T1ToT2[key])
                {
                    T2ToT1[T2Key].Remove(key);
                }
            }

            T1ToT2.Remove(key);
        }
        return success;
    }
    public bool Remove(T2 key)
    {
        var success = T2ToT1.ContainsKey(key);
        if (success)
        {
            if (IsSymmetric)
            {
                foreach (var T1Key in T2ToT1[key])
                {
                    T1ToT2[T1Key].Remove(key);
                }
            }
            T2ToT1.Remove(key);
        }
        return success;
    }


    //******************************************
    public bool TryGetValue(T1 key, out IEnumerable<T2> value)
    {
        var success = T1ToT2.TryGetValue(key, out HashSet<T2> newValue);
        value = newValue ?? new HashSet<T2>();
        return success;
    }
    public bool TryGetValue(T2 key, out IEnumerable<T1> value)
    {
        var success = T2ToT1.TryGetValue(key, out HashSet<T1> newValue);
        value = newValue ?? new HashSet<T1>();
        return success;
    }


    //******************************************
    public void ClearForward()
    {
        if (IsSymmetric)
            Clear();
        else
            T1ToT2.Clear();
    }


    //******************************************
    public void ClearReverse()
    {
        if (IsSymmetric)
            Clear();
        else
            T2ToT1.Clear();
    }


    //******************************************
    public void Clear()
    {
        T1ToT2.Clear();
        T2ToT1.Clear();
    }


    //******************************************
    public static implicit operator TwoWayDictionary<T2, T1>(TwoWayDictionary<T1, T2> other)
    {
        return new TwoWayDictionary<T2, T1>( other.GetHashCode(), other.IsSymmetric, other.T2ToT1, other.T1ToT2);
    }
}
