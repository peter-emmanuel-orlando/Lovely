using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Runtime.Serialization;

#if UNITY_EDITOR
using UnityEditor;
[InitializeOnLoad]
[ExecuteInEditMode]
#endif
[DefaultExecutionOrder(-150)]
public class _Master : MonoBehaviour
{
    public static _Master masterSingleton;
    public static _Master MasterSingleton
    {
        get
        {
            if (masterSingleton == null)
            {
                try { OnLoad(); }
                catch (Exception) { throw new MasterNotFoundInSceneException("there is no master component!"); }
            }
            return masterSingleton;
        }
    }

    static bool isQuitting = false;
    public static bool IsQuitting { get { return isQuitting; } }

    [InitializeOnLoadMethod]
    public static void OnLoad()
    {
        masterSingleton = GameObject.FindObjectOfType<_Master>();
        if (masterSingleton == null)
        {
            masterSingleton = new GameObject("Master").AddComponent<_Master>();
            var gameObject = masterSingleton.gameObject;
        }
        foreach (var type in typeof(MasterComponentBase).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(MasterComponentBase))))
        {
            if (type.IsAbstract) continue;
            if(masterSingleton.gameObject.GetComponent(type) == null)
                masterSingleton.gameObject.AddComponent(type);
        }
    }

    private void Awake()
    {
        var masters = GameObject.FindObjectsOfType<_Master>();
        //if theres already a master
        if (masters.Length > 1)
        {
            DestroyImmediate(this);
        }
        else
        {
            this.gameObject.name = "Master";
            masterSingleton = this;
            foreach (var type in typeof(MasterComponentBase).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(MasterComponentBase))))
            {
                if (type.IsAbstract) continue;
                if (masterSingleton.gameObject.GetComponent(type) == null)
                    masterSingleton.gameObject.AddComponent(type);
            }
        }
    }

    private void Start()
    {
        this.gameObject.name = "Master";
        masterSingleton = this;
        if (Application.isPlaying)
            DontDestroyOnLoad(this.gameObject);
    }

    private void OnApplicationQuit()
    {
        isQuitting = true;
    }
}

public class MasterNotFoundInSceneException : UnityException
{
    public MasterNotFoundInSceneException()
    {
    }

    public MasterNotFoundInSceneException(string message) : base(message)
    {
    }

    public MasterNotFoundInSceneException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected MasterNotFoundInSceneException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
