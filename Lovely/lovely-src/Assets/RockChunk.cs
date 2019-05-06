﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockChunk : ItemsProvider, ISpawnedItem<IStone>
{

    private readonly Type[] itemTypes = new Type[] { typeof(IStone) };
    public override IEnumerable<Type> ItemTypes => itemTypes;

    public override float harvestTime { get; protected set; } = 1;

    public override float harvestCount { get; protected set; } = 1;

    public const string _PrefabName = "RockChunk";
    public string PrefabName => _PrefabName;

    public GameObject GameObject => this.gameObject;

    public Transform Transform => this.transform;

    protected override void Awake()
    {
        base.Awake();
        var rb = gameObject.GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.AddForce(Vector3.up * 2, ForceMode.VelocityChange);
        rb.maxDepenetrationVelocity = 1;
        rb.hideFlags = HideFlags.NotEditable;
        StartCoroutine(ScaleInColliders(2));
    }

    private IEnumerator ScaleInColliders( float lengthInSec)
    {
        var startTime = Time.time;
        var endTime = startTime + lengthInSec;
        var targetScale = transform.localScale;
        var startScale = Vector3.one * 0.05f;
        transform.localScale = startScale;
        while ( Time.time < endTime)
        {
            yield return null;
            var factor = Time.time - startTime / lengthInSec;
            transform.localScale = Vector3.Lerp(startScale, targetScale, factor);
        }
        transform.localScale = targetScale;
        yield break;
    }

    public override bool Acquire<T>(T acquisitioner, out List<IItem> acquiredItems, out List<ISpawnedItem<IItem>> spawnedResources)
    {
        acquiredItems = new List<IItem>();
        while(harvestCount > 0)
        {
            harvestCount--;
            acquiredItems.Add(new StoneItem());
        }
        spawnedResources = new List<ISpawnedItem<IItem>>();
        Destroy(gameObject, 2000);
        return true;
    }
}
