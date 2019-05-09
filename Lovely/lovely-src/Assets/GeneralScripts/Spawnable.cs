﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawnable : MonoBehaviour, ISpawnable
{
    public virtual string PrefabName
    {
        get { return gameObject.name; }
    }
    public GameObject GameObject { get { return (this == null) ? null : base.gameObject; } }
    public Transform Transform { get { return (this == null) ? null : transform; } }
    protected MeshRenderer[] meshRenderers;
    protected SkinnedMeshRenderer[] skinnedMeshRenderers;
    //do same for colliders
    public virtual Bounds Bounds
    {
        get
        {
            var result = new Bounds(transform.position, Vector3.zero);
            foreach (var r in meshRenderers ?? new MeshRenderer[0])
            {
                result.Encapsulate(r.bounds);
            }
            foreach (var s in skinnedMeshRenderers ?? new SkinnedMeshRenderer[0])
            {
                result.Encapsulate(s.bounds);
            }
            return result;
        }
    }

    protected virtual void Start()
    {
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
    }
}
