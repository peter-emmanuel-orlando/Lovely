using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//allows one more item to overfill it
public class Container
{
    readonly TypeStore<HashSet<IItem>> inner = new TypeStore<HashSet<IItem>>();
    public readonly Predicate<IItem> canContainerHoldItem;
    float maxHoldableVolume = 100f;
    public float MaxHoldableVolume { get { return maxHoldableVolume; } }
    float filledVolume = 0;
    public float FilledVolume { get { return filledVolume; } }
    public float FreeVolume { get { return MaxHoldableVolume - FilledVolume; } }

    int itemCount = 0;
    public int ItemCount { get { return itemCount; } }

    public bool isEmpty { get { return itemCount == 0; } }
    public bool isFull { get { return FreeVolume <= 0; } }

    public Container(float maxHoldableVolume)
    {
        this.maxHoldableVolume = maxHoldableVolume;
        this.canContainerHoldItem = delegate (IItem item) { return !isFull; };
    }
    public Container(float maxHoldableVolume, Predicate<IItem> conditionsForContaining)
    {
        this.maxHoldableVolume = maxHoldableVolume;
        this.canContainerHoldItem = delegate (IItem item) { return /*item.Volume <= this.FreeVolume*/ !isFull && conditionsForContaining(item); };;
    }

    public void AddItem()
    {
        throw new NotImplementedException();
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