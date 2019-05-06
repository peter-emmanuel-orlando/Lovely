using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class TypeStore<BaseT>
{
    private TypeStore inner { get; } = new TypeStore();

    public int Count => inner.Count;
    public void Add(BaseT data)
    {
        inner.Add(data);
    }
    public void Add<T>(T data) where T : BaseT
    {
        inner.Add(data);
    }
    public void Remove(BaseT data)
    {
        inner.Remove(data);
    }
    public void Remove<T>(T data) where T : BaseT
    {
        inner.Remove(data);
    }

    public bool ContainsKey(Type type)
    {
        return inner.ContainsKey(type);
    }
    public bool ContainsValue<T>(T data)
    {
        return inner.ContainsValue(data);
    }

    public IEnumerable<BaseT> GetData(bool includeDerivedTypes)
    {
        return inner.GetData<BaseT>(includeDerivedTypes);
    }
    public IEnumerable<T> GetData<T>(bool includeDerivedTypes) where T : BaseT
    {
        return inner.GetData<T>(includeDerivedTypes);
    }
    public IEnumerable<object> GetData(Type t, bool includeDerivedTypes)
    {
        if (t == null || !typeof(BaseT).IsAssignableFrom(t))
            yield break;
        else
            yield return inner.GetData(t, includeDerivedTypes);
    }

    public void Release()
    {
        inner.Release();
    }
    public void Clear()
    {
        inner.Clear();
    }
}
/// <summary>
/// RELEASE MUST BE CALLED OR ELSE MEMORY WILL LEAK
/// </summary>
public class TypeStore
{
    private interface IBaseInterface { }
    private static ClassNode classRoot { get; } = new ClassNode() { BaseType = null, Type = typeof(object) };
    private static InterfaceNode interfaceRoot { get; } = new InterfaceNode() { Type = typeof(IBaseInterface) };
    private static Dictionary<Type, TypeNode> typeNodes { get; } = new Dictionary<Type, TypeNode>();
    static TypeStore()
    {
        typeNodes.Add(interfaceRoot.Type, interfaceRoot);
        typeNodes.Add(classRoot.Type, classRoot);
    }

    public int Count { get; private set; } = 0;

    public void Add<T>(T data)
    {
        var runtime = data.GetType();
        var nodes = new List<TypeNode>(GetOrCreateNode(runtime));
        foreach (var node in nodes)
        {
            if (!node.dataSet.ContainsKey(this))
                node.dataSet.Add(this, new HashSet<object>());
            if (node.dataSet[this].Add(data) && !node.Type.IsInterface)
                this.Count++;
        }
    }
    public void Remove<T>(T data)
    {
        var runtime = data.GetType();
        if (ContainsKey(runtime))
        {

            var minimal = runtime.GetInterfaces().AsEnumerable();
            minimal = minimal.Except(minimal.SelectMany(t => t.GetInterfaces()));
            minimal = minimal.Append(runtime);

            foreach (var type in minimal)
            {
                var node = typeNodes[type];
                if(node.dataSet[this].Remove(data) && !node.Type.IsInterface)
                        this.Count--;
                if (node.dataSet[this].Count == 0)
                    node.dataSet.Remove(this);
                RemoveNodeIfEmpty(node);
            }
        }
    }

    public void Release()
    {
        Clear();
    }

    public void Clear()
    {
        var types = new List<Type>(typeNodes.Keys);//have to make new enumerable as collection may change during iteration
        foreach (var type in types)
        {
            if(typeNodes.ContainsKey(type) && typeNodes[type] != null && typeNodes[type].dataSet.ContainsKey(this))
            {
                typeNodes[type].dataSet.Remove(this);
                RemoveNodeIfEmpty(typeNodes[type]);
            }
        }
        Count = 0;
    }

    public bool ContainsValue<T>(T data)
    {
        var type = data.GetType();
        return typeNodes.ContainsKey(type) && typeNodes[type].dataSet.ContainsKey(this) && typeNodes[type].dataSet[this].Contains(data);
    }
    public bool ContainsKey(Type type)
    {
        return typeNodes.ContainsKey(type);
    }

    public IEnumerable<T> GetData<T>(bool includeDerivedTypes)
    {
        foreach (var data in GetData(typeof(T), includeDerivedTypes))
            yield return (T)data;
    }
    public IEnumerable<object> GetData(Type t, bool includeDerivedTypes)
    {
        if (typeNodes.ContainsKey(t))
        {
            var node = typeNodes[t];
            if(node.dataSet.ContainsKey(this))
            {
                foreach (var data in node.dataSet[this])
                {
                    yield return data;
                }
            }
            if (includeDerivedTypes)
            {
                foreach (var derivedNode in node.DerivedTypes)
                    foreach (var derivedData in GetData(derivedNode.Type, includeDerivedTypes))
                        yield return derivedData;
            }
        }
        yield break;
    }

    private void RemoveNodeIfEmpty(TypeNode subject)
    {
        if (subject.Type.IsInterface)
            RemoveInterfaceNodeIfEmpty((InterfaceNode)subject);
        else
            RemoveClassNodeIfEmpty((ClassNode)subject);
    }
    private void RemoveInterfaceNodeIfEmpty(InterfaceNode subject)
    {
        if (subject.DerivedTypes.Count == 0 && subject.dataSet.Count == 0 && subject != interfaceRoot)
        {
            typeNodes.Remove(subject.Type);
            foreach (var baseNode in subject.BaseTypes)
            {
                baseNode.DerivedTypes.Remove(subject);
                RemoveInterfaceNodeIfEmpty(baseNode);
            }
        }
    }
    private void RemoveClassNodeIfEmpty(ClassNode subject)
    {
        if (subject.DerivedTypes.Count == 0 && subject.dataSet.Count == 0 && subject != classRoot)
        {
            typeNodes.Remove(subject.Type);
            var baseNode = subject.BaseType;
            baseNode.DerivedTypes.Remove(subject);
            RemoveClassNodeIfEmpty(baseNode);
        }
    }

    private IEnumerable<TypeNode> GetOrCreateNode(Type type)
    {
        GetOrCreateClassNodes(type);
        if (type.IsInterface)
            yield return GetOrCreateInterfaceNode(type);
        else
        {
            yield return GetOrCreateClassNodes(type);
            foreach (var item in GetOrCreateInterfaceNodesImplementedByClass(type))
            {
                yield return item;
            }
        }
    }
    private ClassNode GetOrCreateClassNodes(Type classType)
    {
        if (classType == null) throw new ArgumentNullException();
        else if (classType.IsInterface) throw new ArgumentException();
        else if (typeNodes.ContainsKey(classType)) return (ClassNode)typeNodes[classType];
        else
        {
            var newNode = new ClassNode() { Type = classType };
            var baseNode = GetOrCreateClassNodes(classType.BaseType);
            newNode.BaseType = baseNode;
            baseNode.DerivedTypes.Add(newNode);
            typeNodes.Add(classType, newNode);
            return newNode;
        }
    }
    private IEnumerable<InterfaceNode> GetOrCreateInterfaceNodesImplementedByClass(Type classType)
    {
        if (classType == null) throw new ArgumentNullException();
        else if (!classType.IsInterface)
        {
            var minimalInterfaces = classType.GetInterfaces().AsEnumerable();
            minimalInterfaces = minimalInterfaces.Except(minimalInterfaces.SelectMany(t => t.GetInterfaces()));
            foreach (var baseInterface in minimalInterfaces)
            {
                yield return GetOrCreateInterfaceNode(baseInterface);
            }
        }
    }
    private InterfaceNode GetOrCreateInterfaceNode(Type interfaceType)
    {
        if (interfaceType == null) return interfaceRoot;
        else if (interfaceType.IsClass) throw new ArgumentException();
        else if (typeNodes.ContainsKey(interfaceType)) return (InterfaceNode)typeNodes[interfaceType];
        else
        {
            InterfaceNode newNode = new InterfaceNode() { Type = interfaceType };

            var minimalInterfaces = interfaceType.GetInterfaces().Where(t => t != interfaceType);
            minimalInterfaces = minimalInterfaces.Except(minimalInterfaces.SelectMany(t => t.GetInterfaces()));
            /* dont remove interfaces from base types. this prevents not storing item because interface wasnt on most specicic class
            var current = type.BaseType;
            while (current != null)
            {
                minimalInterfaces = minimalInterfaces.Except(current.GetInterfaces().SelectMany(t => t.GetInterfaces()));
                current = current.BaseType;
            }
            */
            if (interfaceType.IsGenericType)
            {
                var hasVariance = false;
                var genericType = interfaceType.GetGenericTypeDefinition();
                var genericTypeArgs = genericType.GetGenericArguments();
                foreach (var item in genericTypeArgs)
                {
                    hasVariance = (item.GenericParameterAttributes & GenericParameterAttributes.VarianceMask) != GenericParameterAttributes.None;
                    if (hasVariance && genericTypeArgs.Length > 1)
                        throw new NotSupportedException("currently the typestore only accepts covariant or contravariant interfaces with 1 generic parameter!");
                }

                if (hasVariance)
                {
                    var constructedType = interfaceType.GetGenericArguments()[0];
                    var constraints = genericTypeArgs[0].GetGenericParameterConstraints();
                    var variantBaseTypes = constructedType.GetInterfaces().Where(t => t != constructedType && constraints.IsAnyAssignableFrom(t));
                    variantBaseTypes = variantBaseTypes.Except(variantBaseTypes.SelectMany(t => t.GetInterfaces()));
                    //need to check for reference, notNullable, defaultConstructor constraints 

                    foreach (var variantBase in variantBaseTypes)
                    {
                        var additionalInterface = genericType.MakeGenericType(variantBase);
                        minimalInterfaces = minimalInterfaces.Append(additionalInterface);
                    }
                }
            }

            var v = new List<Type>(minimalInterfaces);
            var w = "";

            if (minimalInterfaces.Count() == 0)
            {
                newNode.BaseTypes.Add(interfaceRoot);
                interfaceRoot.DerivedTypes.Add(newNode);
            }
            else
            {
                foreach (var baseInterface in minimalInterfaces)
                {
                    var baseNode = GetOrCreateInterfaceNode(baseInterface);
                    newNode.BaseTypes.Add(baseNode);
                    baseNode.DerivedTypes.Add(newNode);
                }
            }
            typeNodes.Add(interfaceType, newNode);
            return newNode;
        }
    }


    private class TypeNode
    {
        public Type Type { get; set; }
        public Dictionary<TypeStore, HashSet<object>> dataSet { get; } = new Dictionary<TypeStore, HashSet<object>>();
        public HashSet<TypeNode> DerivedTypes { get; } = new HashSet<TypeNode>();
    }
    private class ClassNode : TypeNode
    {
        public ClassNode BaseType { get; set; }
    }
    private class InterfaceNode : TypeNode
    {
        public HashSet<InterfaceNode> BaseTypes { get; } = new HashSet<InterfaceNode>();
    }
}

public static class TypeIEnumerableHelper
{
    public static bool IsAnyAssignableFrom(this IEnumerable<Type> baseTypes, Type potentiallyDerivedTypes)
    {
        if (potentiallyDerivedTypes == null)
            return false;

        foreach (var baseType in baseTypes)
        {
            if (baseType.IsAssignableFrom(potentiallyDerivedTypes))
                return true;
        }

        return false;
    }
    public static bool IsAssignableFromAny(this Type baseType, IEnumerable<Type> potentiallyDerivedTypes)
    {
        if (potentiallyDerivedTypes == null)
            return false;

        foreach (var potentiallyDerivedType in potentiallyDerivedTypes)
        {
            if (baseType.IsAssignableFrom(potentiallyDerivedType))
                return true;
        }

        return false;
    }


    /// <summary>
    /// returns true for itself
    /// </summary>
    public static bool ExtendsFromAny(this Type type, IEnumerable<Type> baseTypes)
    {
        if (baseTypes == null)
            return false;

        foreach (var baseType in baseTypes)
        {
            if (baseType.IsAssignableFrom(type))
                return true;
        }

        return false;
    }
    /// <summary>
    /// returns true for itself
    /// </summary>
    public static bool DoesAnyExtendFrom(this IEnumerable<Type> types, Type baseType)
    {
        if (baseType == null)
            return false;

        foreach (var type in types)
        {
            if (baseType.IsAssignableFrom(type))
                return true;
        }

        return false;
    }
}

