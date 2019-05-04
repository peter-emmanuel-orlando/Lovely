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
        if (t == null || !t.IsAssignableFrom(typeof(BaseT)))
            yield break;
        else
            yield return inner.GetData(t, includeDerivedTypes);
    }
}
public class TypeStore
{
    private ClassNode classRoot = new ClassNode() { BaseType = null, Type = typeof(object) };
    private InterfaceNode interfaceRoot = new InterfaceNode() { Type = typeof(IBaseInterface) };
    private Dictionary<Type, TypeNode> typeNodes = new Dictionary<Type, TypeNode>();
    public int Count { get; private set; } = 0;
    //private Dictionary<Type, ClassNode> classNodes = new Dictionary<Type, ClassNode>();
    //private Dictionary<Type, InterfaceNode> interfaceNodes = new Dictionary<Type, InterfaceNode>();

    private interface IBaseInterface { }
    public TypeStore()
    {
        typeNodes.Add(interfaceRoot.Type, interfaceRoot);
        typeNodes.Add(classRoot.Type, classRoot);
    }



    public void Add<T>(T data)
    {
        var runtime = data.GetType();
        var nodes = new List<TypeNode>(GetOrCreateNode(runtime));
        foreach (var node in nodes)
        {
            node.dataSet.Add(data);
            if (node.dataSet.Count == 1 && !node.Type.IsInterface)
                this.Count++;
        }
    }
    public void Remove<T>(T data)
    {
        var runtime = data.GetType();
        if (typeNodes.ContainsKey(runtime) && typeNodes[runtime].dataSet.Contains(data))
        {
            var node = typeNodes[runtime];
            node.dataSet.Remove(data);
            if (node.dataSet.Count == 0 && !node.Type.IsInterface)
                this.Count--;
            RemoveNodeIfEmpty(typeNodes[runtime]);
        }
    }

    public bool ContainsValue<T>(T data)
    {
        var type = typeof(T);
        return typeNodes.ContainsKey(type) && typeNodes[type].dataSet.Contains(data);
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
            foreach (var data in node.dataSet)
            {
                yield return data;
                if (includeDerivedTypes)
                {
                    foreach (var derivedNode in node.DerivedTypes)
                        foreach (var derivedData in GetData(derivedNode.Type, includeDerivedTypes))
                            yield return derivedData;
                }
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
                    var variantBaseTypes = constructedType.GetInterfaces().Where(t => t != constructedType);
                    variantBaseTypes = variantBaseTypes.Except(variantBaseTypes.SelectMany(t => t.GetInterfaces()));

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
        public HashSet<object> dataSet { get; } = new HashSet<object>();
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
    public static bool IsAnyAssignableFrom(this IEnumerable<Type> types, Type search)
    {
        if (search == null)
            return false;

        foreach (var type in types)
        {
            if (type.IsAssignableFrom(search))
                return true;
        }

        return false;
    }
    /// <summary>
    /// returns true for itself
    /// </summary>
    public static bool IsBaseOfAny(this Type type, IEnumerable<Type> searches)
    {
        if (searches == null)
            return false;

        foreach (var search in searches)
        {
            if (search.IsAssignableFrom(type))
                return true;
        }

        return false;
    }
}

