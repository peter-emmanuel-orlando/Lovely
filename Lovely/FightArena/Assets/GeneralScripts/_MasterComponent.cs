using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public abstract class MasterComponentBase : MonoBehaviour { }
public abstract class _MasterComponent<T> : MasterComponentBase where T : Component
{
        static _Master master;
    private static T instance;
    protected static T Instance
    {
        get
        {
            if(instance == null)
            {
                master = _Master.MasterSingleton;
                if (master == null)
                    throw new MasterNotFoundInSceneException();

                instance = master.GetComponent<T>();
                if (instance == null)
                    throw new MasterComponentNotFoundInSceneException("there is no component of " + typeof(T) + "in the scene!");                
            }
            return instance;
        }
    }

    protected virtual void Awake()
    {
        _Master.OnLoad();
        if (master == null)
        {
            try
            {
                master = _Master.MasterSingleton;
            }
            catch (MasterNotFoundInSceneException e)
            {
                DestroyImmediate(this);
                throw e;
            }
        }

        instance = master.GetComponent<T>();
        if(instance != this)
            DestroyImmediate(this);
        if(instance == null)
            instance = master.gameObject.AddComponent<T>();
    }
}

public class MasterComponentNotFoundInSceneException : UnityException
{
    public MasterComponentNotFoundInSceneException()
    {
    }

    public MasterComponentNotFoundInSceneException(string message) : base(message)
    {
    }

    public MasterComponentNotFoundInSceneException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected MasterComponentNotFoundInSceneException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}




/*
    using UnityEditor;
    [InitializeOnLoad]
    [InitializeOnLoadMethod]
    public static void OnLoad()
    {
        _Master.OnLoad();
        master = _Master.MasterSingleton;
        master.gameObject.AddComponent<T>();

    }
*/
