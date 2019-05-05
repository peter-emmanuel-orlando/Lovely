using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneSlab : ItemsProvider, IItemsProvider<IStone>
{

    private readonly Type[] itemTypes = new Type[] { typeof(IStone) };
    public override IEnumerable<Type> ItemTypes => itemTypes;

    public override float harvestTime => 1;

    public override float harvestCount => 10;

    private void OnEnable()
    {
        TrackedComponent.Track(this);
    }

    private void OnDisable()
    {
        TrackedComponent.Untrack(this);   
    }

    public override bool Acquire<T>(T acquisitioner, out List<IItem> acquiredItems, out List<ISpawnedItem<IItem>> spawnedResources)
    {
        acquiredItems = new List<IItem>();
        spawnedResources = new List<ISpawnedItem<IItem>>();
        return true;
    }
}
public class RockChunk : ItemsProvider, IItemsProvider<IStone>
{
    public override IEnumerable<Type> ItemTypes => throw new NotImplementedException();

    public override float harvestTime => throw new NotImplementedException();

    public override float harvestCount => throw new NotImplementedException();

    public override bool Acquire<T>(T acquisitioner, out List<IItem> acquiredItems, out List<ISpawnedItem<IItem>> spawnedResources)
    {
        throw new NotImplementedException();
    }
}