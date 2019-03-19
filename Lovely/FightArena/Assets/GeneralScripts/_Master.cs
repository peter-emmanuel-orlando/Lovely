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
public class _Master : MonoBehaviour
{
    public static _Master master;
    public static _Master MasterSingleton
    {
        get
        {
            if (master == null)
            {
                try
                {
                    OnLoad();
                }
                catch(Exception e)
                {
                    throw new MasterNotFoundInSceneException("there is no master component!");
                }

                }
            return master;
        }
    }

    [InitializeOnLoadMethod]
    public static void OnLoad()
    {
        master = GameObject.FindObjectOfType<_Master>();
        if (master == null)
        {
            master = new GameObject("Master").AddComponent<_Master>();
            var gameObject = master.gameObject;
        }
        foreach (var type in typeof(MasterComponentBase).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(MasterComponentBase))))
        {
            if (type.IsAbstract) continue;
            if(master.gameObject.GetComponent(type) == null)
                master.gameObject.AddComponent(type);
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
            master = this;
        }
    }

    private void Start()
    {
        this.gameObject.name = "Master";
        master = this;
        if (Application.isPlaying)
            DontDestroyOnLoad(this.gameObject);
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
