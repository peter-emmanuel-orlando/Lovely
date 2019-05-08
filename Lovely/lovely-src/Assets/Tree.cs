using System;
using System.Collections.Generic;

public class Tree : ItemsProvider, IItemsProvider<IWood>, IItemsProvider<ILeaves>
{

    private readonly Type[] itemTypes = new Type[] { typeof(IWood), typeof(ILeaves) };
    public override IEnumerable<Type> ItemTypes => itemTypes;

    public override float? harvestTime { get; protected set; } = 10;

    public override float harvestCount { get; protected set; } = 20;

    public override bool Acquire<T>(T acquisitioner, out List<IItem> acquiredItems, out List<ISpawnedItem<IItem>> spawnedResources, bool requestSuccessOverride = false)
    {
        Func<IItem[]> getAcquiredItemInstances = () => new WoodItem[] { new WoodItem() };

        Func<ISpawnedItem<IItem>[]> getSpawnedItemInstances = () =>
        {
            var l = new List<ISpawnedItem<IItem>>();
            for (int i = 0; i < 2; i++)
                l.Add(Instantiate(_PrefabPool.GetPrefab(WoodChunk._PrefabName).GameObject, transform.position, transform.rotation).GetComponent<WoodChunk>());
            for (int i = 0; i < 5; i++)
                l.Add(Instantiate(_PrefabPool.GetPrefab(LeafPile._PrefabName).GameObject, transform.position, transform.rotation).GetComponent<LeafPile>());
            return l.ToArray();
        };

        return base.Acquire(acquisitioner, out acquiredItems, out spawnedResources, requestSuccessOverride, getSpawnedItemInstances, getAcquiredItemInstances);
    }
}
