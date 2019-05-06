﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class StoneSlab : ItemsProvider, IItemsProvider<IStone>
{

    private readonly Type[] itemTypes = new Type[] { typeof(IStone) };
    public override IEnumerable<Type> ItemTypes => itemTypes;

    public override float? harvestTime { get; protected set; } = 5;

    public override float harvestCount { get; protected set; } = 10;

    public override bool Acquire<T>(T acquisitioner, out List<IItem> acquiredItems, out List<ISpawnedItem<IItem>> spawnedResources, bool requestSuccessOverride = false)
    {
        acquiredItems = new List<IItem>();
        spawnedResources = new List<ISpawnedItem<IItem>>();
        if(!requestSuccessOverride)
        {
            if (harvestCount <= 0) return false;
            if (typeof(IBounded).IsAssignableFrom(typeof(T)) && !((IBounded)acquisitioner).Bounds.Intersects(this.Bounds)) return false;
        }
        var rockPrefab = _PrefabPool.GetPrefab(RockChunk._PrefabName);
        while (harvestCount > 0)
        {
            harvestCount--;
            var spawnedRocks = Instantiate(rockPrefab.GameObject, transform.position, transform.rotation).GetComponent<RockChunk>();
            spawnedResources.Add(spawnedRocks);
        }
        if (this.harvestCount <= 0)
        {
            Destroy(gameObject);
        }
        return true;
    }
}
public class StoneItem : Item, IStone
{
    public override Type ItemType => typeof(IStone);

    public override float Volume => 0.2f;

    public override MatterPhase Phase => MatterPhase.Solid;
}
