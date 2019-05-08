using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum TypeIncludeMode
{
    OnlyThis = 0,
    IncludeBaseTypes,
    IncludeExtendingTypes,
}
public class TypeDictionary<T> : IDictionary<Type, T>
{
    public T this[Type key]
    {
        get => ((IDictionary<Type, T>)inner)[key];
        set => ((IDictionary<Type, T>)inner)[key] = value;
    }

    public IEnumerable<T> Get(Type key, TypeIncludeMode include = TypeIncludeMode.OnlyThis)
    {
        List<Type> types;
        if(include == TypeIncludeMode.IncludeExtendingTypes)
        {
            //AppDomain.CurrentDomain.GetAssemblies()
            types = new List<Type>( key.Assembly.GetTypes().Where(type => type.IsAssignableFrom(key)));
        }
        else if (include == TypeIncludeMode.IncludeBaseTypes)
        {
            types = new List<Type>(key.BaseTypes());
        }
        else
        {
            types = new List<Type>(new Type[] { key });
        }

        foreach (var type in types)
        {
            if (inner.ContainsKey(type))
                yield return inner[type];
        }
        yield break;
    }

    public ICollection<Type> Keys => ((IDictionary<Type, T>)inner).Keys;

    public ICollection<T> Values => ((IDictionary<Type, T>)inner).Values;

    public int Count => ((IDictionary<Type, T>)inner).Count;

    public bool IsReadOnly => ((IDictionary<Type, T>)inner).IsReadOnly;

    Dictionary<Type, T> inner { get; } = new Dictionary<Type, T>();

    public void Add(Type key, T value)
    {
        ((IDictionary<Type, T>)inner).Add(key, value);
    }

    public void Add(KeyValuePair<Type, T> item)
    {
        ((IDictionary<Type, T>)inner).Add(item);
    }

    public void Clear()
    {
        ((IDictionary<Type, T>)inner).Clear();
    }

    public bool Contains(KeyValuePair<Type, T> item)
    {
        return ((IDictionary<Type, T>)inner).Contains(item);
    }

    public bool ContainsKey(Type key)
    {
        return ((IDictionary<Type, T>)inner).ContainsKey(key);
    }

    public void CopyTo(KeyValuePair<Type, T>[] array, int arrayIndex)
    {
        ((IDictionary<Type, T>)inner).CopyTo(array, arrayIndex);
    }

    public IEnumerator<KeyValuePair<Type, T>> GetEnumerator()
    {
        return ((IDictionary<Type, T>)inner).GetEnumerator();
    }

    public bool Remove(Type key)
    {
        return ((IDictionary<Type, T>)inner).Remove(key);
    }

    public bool Remove(KeyValuePair<Type, T> item)
    {
        return ((IDictionary<Type, T>)inner).Remove(item);
    }

    public bool TryGetValue(Type key, out T value)
    {
        return ((IDictionary<Type, T>)inner).TryGetValue(key, out value);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IDictionary<Type, T>)inner).GetEnumerator();
    }
}
