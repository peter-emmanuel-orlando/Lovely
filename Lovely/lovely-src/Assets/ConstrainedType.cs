using System;
using System.Collections.Generic;


public interface IConstrainedType<out T>
{
    Type type { get; }
}
public class ConstrainedType<TBase> : IConstrainedType<TBase>
{
    public Type type { get; }
    public ConstrainedType() : this(typeof(TBase)) { }
    public ConstrainedType( Type type)
    {
        var v = new List<IConstrainedType<Mind>>();
        v.Add(new ConstrainedType<PerceivingMind>());
        this.type = type;
    }

    public static implicit operator Type(ConstrainedType<TBase> subject)
    {
        return subject.type;
    }

    public static implicit operator ConstrainedType<TBase> (Type subject)
    {
        if (subject.IsAssignableFrom(typeof(TBase)))
            return new ConstrainedType<TBase>(subject);
        else
            throw new InvalidCastException($"{subject} is not assignable from {typeof(TBase)}!");
    }

    public override string ToString()
    {
        return type.ToString();
    }

    public override bool Equals(object obj)
    {
        return type.Equals(obj);
    }

    public override int GetHashCode()
    {
        return type.GetHashCode();
    }
}
