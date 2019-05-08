using System;
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
        Func<IItem[]> getAcquiredItemInstances = () => new StoneItem[] { new StoneItem() };

        var rockPrefab = _PrefabPool.GetPrefab(RockChunk._PrefabName);//<-
        Func<ISpawnedItem<IItem>[]> getSpawnedItemInstances = () =>
        {
            var l = new List<RockChunk>();
            for (int i = 0; i < 2; i++)
                l.Add( Instantiate(rockPrefab.GameObject, transform.position, transform.rotation).GetComponent<RockChunk>());
            return l.ToArray();
        };

        return base.Acquire(acquisitioner, out acquiredItems, out spawnedResources, requestSuccessOverride, getSpawnedItemInstances, getAcquiredItemInstances);
    }
}

