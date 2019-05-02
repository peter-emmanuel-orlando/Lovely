using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TypeStore<BaseT>
{
    private TypeStore inner { get; } = new TypeStore();
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
        var node = GetOrCreateNode(runtime);
        node.dataSet.Add(data);
    }
    public void Remove<T>(T data)
    {
        var runtime = data.GetType();
        if (typeNodes.ContainsKey(runtime) && typeNodes[runtime].dataSet.Contains(data))
        {
            typeNodes[runtime].dataSet.Remove(data);
            RemoveNodeIfEmpty(typeNodes[runtime]);
        }
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
                if(includeDerivedTypes)
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

    private TypeNode GetOrCreateNode(Type type)
    {
        GetOrCreateClassNodes(type);
        if (type.IsInterface)
            return GetOrCreateInterfaceNode(type);
        else
            return GetOrCreateClassNodes(type);
    }
    private ClassNode GetOrCreateClassNodes(Type type)
    {
        if (type == null) throw new ArgumentNullException();
        else if (type.IsInterface) throw new ArgumentException();
        else if (typeNodes.ContainsKey(type)) return (ClassNode)typeNodes[type];
        else
        {
            var newNode = new ClassNode() { Type = type };
            var baseNode = GetOrCreateClassNodes(type.BaseType);
            newNode.BaseType = baseNode;
            baseNode.DerivedTypes.Add(newNode);
            return newNode;
        }
    }
    private InterfaceNode GetOrCreateInterfaceNode(Type type)
    {
        if (type == null) return interfaceRoot;
        else if (type.IsClass) throw new ArgumentException();
        else if (typeNodes.ContainsKey(type)) return (InterfaceNode)typeNodes[type];
        else
        {
            InterfaceNode newNode = new InterfaceNode() { Type = type };

            var minimalInterfaces = type.GetInterfaces().Where(t => t != type);
            var current = type.BaseType;
            while (current != null)
            {
                minimalInterfaces = minimalInterfaces.Except(current.GetInterfaces().SelectMany(t => t.GetInterfaces()));
                current = current.BaseType;
            }
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
    public static bool IsAssignableFromAny(this IEnumerable<Type> types, Type search)
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

