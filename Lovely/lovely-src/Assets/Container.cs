using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//allows one more item to overfill it
public class Container
{
    readonly TypeStoreUNMANAGED<IItem> inner = new TypeStoreUNMANAGED<IItem>();
    public readonly Predicate<IItem> canContainerHoldItem;

    public float MaxHoldableVolume { get; } = 100f;
    public float FilledVolume { get; } = 0;
    public float FreeVolume { get { return MaxHoldableVolume - FilledVolume; } }

    public int ItemCount => inner.Count;

    public bool isEmpty { get { return ItemCount == 0; } }
    public bool isFull { get { return FreeVolume <= 0; } }

    public Container(float maxHoldableVolume)
    {
        this.MaxHoldableVolume = maxHoldableVolume;
        this.canContainerHoldItem = delegate (IItem item) { return !isFull; };
    }
    public Container(float maxHoldableVolume, Predicate<IItem> conditionsForContaining)
    {
        this.MaxHoldableVolume = maxHoldableVolume;
        this.canContainerHoldItem = delegate (IItem item) { return /*item.Volume <= this.FreeVolume*/ !isFull && conditionsForContaining(item); };
    }

    public void AddItem<T>( T item) where T : IItem
    {
        //throw new NotImplementedException();
        //if item already exists, add together volumes
        if(item is IDivisibleItem<T> && inner.ContainsKey(item.GetType()))
        {
            var preExisting = inner.GetData(item.GetType(), false).First() as IDivisibleItem<T>;
            preExisting.Combine(ref item);
        }
        else
            inner.Add(item);
    }

    public IItem GetItem()
    {
        throw new NotImplementedException();
    }
    public string GetItemsList()
    {
        throw new NotImplementedException();
    }

    //fill with food if its a pantry, or gas if its a tank...
    public bool GetMaintinenceAssignment(Body being, ref IPerformable assignment)
    {
        var assignedPerformable = false;
        return assignedPerformable;
    }

}