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
        acquiredItems = new List<IItem>();
        spawnedResources = new List<ISpawnedItem<IItem>>();
        if (!requestSuccessOverride)
        {
            if (harvestCount <= 0) return false;
            if (typeof(IBounded).IsAssignableFrom(typeof(T)) && !((IBounded)acquisitioner).Bounds.Intersects(this.Bounds)) return false;
        }
        var spawnPrefab = _PrefabPool.GetPrefab(WoodChunk._PrefabName);
        while (harvestCount > 0)
        {
            harvestCount--;
            var newSpawned = Instantiate(spawnPrefab.GameObject, transform.position, transform.rotation).GetComponent<ISpawnedItem<IWood>>();
            spawnedResources.Add(newSpawned);
        }
        if (this.harvestCount <= 0)
        {
            Destroy(gameObject);
        }
        return true;
    }
}
public class WoodItem : Item, IWood
{
    public override Type ItemType => typeof(IWood);

    public override float Volume => 0.2f;

    public override MatterPhase Phase => MatterPhase.Solid;
}
public class LeavesItem : Item, ILeaves
{
    public override  Type ItemType => typeof(ILeaves);

    public override float Volume => 0.2f;

    public override  MatterPhase Phase => MatterPhase.Solid;
}
