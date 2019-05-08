using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
public class TypeLimiter<TBase>
{
    public Type Type { get; }
    private TypeLimiter(Type type)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));
        else if (!typeof(TBase).IsAssignableFrom(type))
            throw new InvalidCastException($"{type} is not derived from {typeof(TBase)}!");

        Type = type;
    }

    public static implicit operator TypeLimiter<TBase>(Type subject)
    {
        return new TypeLimiter<TBase>(subject);
    }

    public static implicit operator Type(TypeLimiter<TBase> subject)
    {
        return subject.Type;
    }
}
/*
public class CType<T> : CType
{
    public CType() : base (typeof(T)){}
}
public class CType
{
    public Type Type { get; }
    public CType(Type type)
    {
        Type = type;
    }
}
public class TypeLimiter<TBase> : Type, IEquatable<TypeLimiter<TBase>>
{
    public Type Type { get; }

    public override Assembly Assembly => Type.Assembly;

    public override string AssemblyQualifiedName => Type.AssemblyQualifiedName;

    public override Type BaseType => Type.BaseType;

    public override string FullName => Type.FullName;

    public override Guid GUID => Type.GUID;

    public override Module Module => Type.Module;

    public override string Name => Type.Name;

    public override string Namespace => Type.Namespace;

    public override Type UnderlyingSystemType => Type.UnderlyingSystemType;

    private TypeLimiter(Type type)
    {
        if(type == null)
            throw new ArgumentNullException(nameof(type));
        else if (!type.IsAssignableFrom(typeof(TBase)))
            throw new ArgumentException("provided type must extend from " + typeof(TBase));

        Type = type;
    }

    public static implicit operator TypeLimiter<TBase>(CType subject)
    {
        if (subject.Type.IsAssignableFrom(typeof(TBase)))
            return new TypeLimiter<TBase>(subject.Type);
        else
            throw new InvalidCastException($"{subject} is not assignable from {typeof(TBase)}!");
    }

    public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
    {
        return Type.GetConstructors(bindingAttr);
    }

    public override object[] GetCustomAttributes(bool inherit)
    {
        return Type.GetCustomAttributes(inherit);
    }

    public override object[] GetCustomAttributes(Type attributeType, bool inherit)
    {
        return Type.GetCustomAttributes(attributeType, inherit);
    }

    public override Type GetElementType()
    {
        return Type.GetElementType();
    }

    public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
    {
        return Type.GetEvent(name, bindingAttr);
    }

    public override EventInfo[] GetEvents(BindingFlags bindingAttr)
    {
        return Type.GetEvents(bindingAttr);
    }

    public override FieldInfo GetField(string name, BindingFlags bindingAttr)
    {
        return Type.GetField(name, bindingAttr);
    }

    public override FieldInfo[] GetFields(BindingFlags bindingAttr)
    {
        return Type.GetFields(bindingAttr);
    }

    public override Type GetInterface(string name, bool ignoreCase)
    {
        return Type.GetInterface(name, ignoreCase);
    }

    public override Type[] GetInterfaces()
    {
        return Type.GetInterfaces();
    }

    public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
    {
        return Type.GetMembers(bindingAttr);
    }

    public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
    {
        return Type.GetMethods(bindingAttr);
    }

    public override Type GetNestedType(string name, BindingFlags bindingAttr)
    {
        return Type.GetNestedType(name, bindingAttr);
    }

    public override Type[] GetNestedTypes(BindingFlags bindingAttr)
    {
        return Type.GetNestedTypes(bindingAttr);
    }

    public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
    {
        return Type.GetProperties(bindingAttr);
    }

    public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
    {
        return Type.InvokeMember(name, invokeAttr, binder, target, args, modifiers, culture, namedParameters);
    }

    public override bool IsDefined(Type attributeType, bool inherit)
    {
        return Type.IsDefined(attributeType, inherit);
    }

    protected override TypeAttributes GetAttributeFlagsImpl()
    {
        throw new NotImplementedException();
    }

    protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
    {
        throw new NotImplementedException();
    }

    protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
    {
        throw new NotImplementedException();
    }

    protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
    {
        throw new NotImplementedException();
    }

    protected override bool HasElementTypeImpl()
    {
        throw new NotImplementedException();
    }

    protected override bool IsArrayImpl()
    {
        throw new NotImplementedException();
    }

    protected override bool IsByRefImpl()
    {
        throw new NotImplementedException();
    }

    protected override bool IsCOMObjectImpl()
    {
        throw new NotImplementedException();
    }

    protected override bool IsPointerImpl()
    {
        throw new NotImplementedException();
    }

    protected override bool IsPrimitiveImpl()
    {
        throw new NotImplementedException();
    }



    public override bool Equals(object obj)
    {
        return Type.Equals(obj);
    }

    public bool Equals(TypeLimiter<TBase> other)
    {
        return Type.Equals(other);
    }

    public override int GetHashCode()
    {
        return Type.GetHashCode();
    }

    public static bool operator ==(TypeLimiter<TBase> left, Type right)
    {
        return left.Type == right;
    }

    public static bool operator !=(TypeLimiter<TBase> left, Type right)
    {
        return !(left.Type == right);
    }

    public static bool operator ==(Type left, TypeLimiter<TBase> right)
    {
        return left == right.Type;
    }

    public static bool operator !=(Type left, TypeLimiter<TBase> right)
    {
        return !(left == right.Type);
    }

    public override string ToString()
    {
        return Type.ToString();
    }
}
/*
/// <summary>
/// any of the inheriting types will match this
/// </summary>
/// <typeparam name="T"></typeparam>
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
*/
