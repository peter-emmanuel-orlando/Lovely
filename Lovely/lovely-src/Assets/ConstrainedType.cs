using System;
using System.Collections.Generic;


public interface IConstrainedType<out T>
{
    Type type { get; }
}

/// <summary>
/// the variable holding the generic type should be IConstrainedType, not the class!!!
/// </summary>
public class ConstrainedType<TBase> : ConstrainedType, IConstrainedType<TBase>
{
    public ConstrainedType() : base(typeof(TBase)) { }
    public ConstrainedType(Type type) : base(type) { }

    public static implicit operator Type(ConstrainedType<TBase> subject)
    {
        return subject.type;
    }
    public static implicit operator ConstrainedType<TBase>(Type subject)
    {
        if (subject.IsAssignableFrom(typeof(TBase)))
            return new ConstrainedType<TBase>(subject);
        else
            throw new InvalidCastException($"{subject} is not assignable from {typeof(TBase)}!");
    }
}
public class ConstrainedType : IConstrainedType<object>
{
    public Type type { get; }
    public ConstrainedType( Type type)
    {
        this.type = type;
    }

    public static implicit operator Type(ConstrainedType subject)
    {
        return subject.type;
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
