using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone : MonoBehaviour, IItemsProvider<IConstructionMaterial>
{

    public IEnumerable<Type> ItemTypes => new Type[] { typeof(IConstructionMaterial) }; 

    public Bounds Bounds => new Bounds(transform.position, Vector3.one * 10);

    public bool Acquire<TAcquisitioner>(TAcquisitioner acquisitioner, out List<IItem> acquiredItems, out List<ISpawnedItem> spawnedResources)
    {
        throw new NotImplementedException();
    }

    public AcquireItemPerformable GetInteractionPerformable(Body performer)
    {
        throw new NotImplementedException();
    }

    private void OnEnable()
    {
        TrackedComponent.Track(this);
    }

    private void OnDisable()
    {
        TrackedComponent.Untrack(this);   
    }
    
}
