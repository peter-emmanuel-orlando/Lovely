using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemRecipie
{

    public TypeStoreUNMANAGED sorted { get; } = new TypeStoreUNMANAGED();
    private HashSet<IItemRequirement> ItemRequirements { get; }
    protected ItemRecipie(HashSet<IItemRequirement> itemRequirements)
    {
        ItemRequirements = itemRequirements;
        foreach (var item in ItemRequirements)
        {
            sorted.Add(item);
        }
    }
    public float CurrentItemsValue
    {
        get
        {
            var result = 0f;
            foreach (var item in ItemRequirements)
            {
                result += item.CurrentValue;
            }
            return result;
        }
    }
    public float TotalRecipieCost
    {
        get
        {
            var result = 0f;
            foreach (var item in ItemRequirements)
            {
                result += item.TotalNeededValue;
            }
            return result;
        }
    }

    public bool HasNeccessaryItemsToCraft(Container items)
    {
        var result = true;
        foreach (var kvPair in ItemRequirements)
        {
            //var Item = items.
        }
        return result;
    }
    /*
    public bool MeetsQualificationsToCraft<T>( T crafter)
    {
        return false;
    }

    public bool InLocationToCraft(Vector3 location)
    {
        return false;
    }
    */

    // public bool 
}

//in progress item

public enum ItemFulfilmentStatus
{
    NotFulfilled = 0,
    Partial,
    Fulfilled
}

public interface IItemRequirement
{
    float CurrentValue { get; }
    float CurrentVolume { get; }
    bool IsDivisible { get; }
    ItemFulfilmentStatus IsSatisfied { get; }
    float RemainingNeededValue { get; }
    float RemainingNeededVolume { get; }
    Type RequirementType { get; }
    float TotalNeededValue { get; }
    float TotalNeededVolume { get; }

    ItemFulfilmentStatus Give(IItem item);
}
public class ItemRequirement<T> : IItemRequirement where T : IItem
{
    private class RequirementPlaceholder : Item
    {
        public override Type ItemType { get; }

        public override float Volume { get; protected set; }

        public override float ValuePerVolume { get; }

        public override MatterPhase Phase { get; }
        public RequirementPlaceholder(Type itemType, float volume, float valuePerVolume, MatterPhase phase)
        {
            ItemType = itemType ?? throw new ArgumentNullException(nameof(itemType));
            Volume = volume;
            ValuePerVolume = valuePerVolume;
            Phase = phase;
        }

        public void SetVolume(float newVolume) { Volume = newVolume; }
    }

    private readonly RequirementPlaceholder internalItem;
    public Type RequirementType => internalItem.ItemType;
    public bool IsDivisible { get; }
    public float TotalNeededVolume { get; }
    public float TotalNeededValue { get; }
    public float CurrentVolume => internalItem.Volume;
    public float CurrentValue => internalItem.Value;
    public float RemainingNeededVolume => Mathf.Max(TotalNeededVolume - CurrentVolume, RemainingNeededValue / internalItem.ValuePerVolume);
    public float RemainingNeededValue => Mathf.Max(TotalNeededValue - CurrentValue, RemainingNeededVolume * internalItem.ValuePerVolume);

    public ItemFulfilmentStatus IsSatisfied
    {
        get
        {
            if (CurrentVolume >= TotalNeededVolume && CurrentValue >= TotalNeededValue)
                return ItemFulfilmentStatus.Fulfilled;
            else if (CurrentVolume > 0 || CurrentValue > 0)
                return ItemFulfilmentStatus.Partial;
            else
                return ItemFulfilmentStatus.NotFulfilled;
        }
    }
    public ItemRequirement(float totalNeededVolume, float totalNeededValue, T template)
    {
        if(totalNeededValue > totalNeededVolume * template.ValuePerVolume)
        {
            TotalNeededValue = totalNeededValue;
            TotalNeededVolume = totalNeededValue / template.ValuePerVolume;
        }
        else
        {
            TotalNeededVolume = totalNeededVolume;
            TotalNeededValue = totalNeededVolume * template.ValuePerVolume;
        }
        IsDivisible = typeof(IDivisibleItem<T>).IsAssignableFrom(typeof(T));
        internalItem = new RequirementPlaceholder(template.ItemType, template.Volume, template.ValuePerVolume, template.Phase);
    }

    public ItemFulfilmentStatus Give(IItem item)
    {
        if (item.ItemType == RequirementType)
        {
            if (IsDivisible)
            {
                var requirement = Mathf.Max(RemainingNeededVolume, RemainingNeededValue / item.ValuePerVolume);
                var useVolume = Mathf.Min(item.Volume, requirement);
                internalItem.SetVolume(internalItem.Volume + useVolume);
                ((IDivisibleItem<T>)item).UseVolume(useVolume);
            }
            else if (item.Volume > TotalNeededVolume && item.Value > TotalNeededValue)
            {
                internalItem.SetVolume(internalItem.Volume);
                item.UseItem();
            }
        }
        return IsSatisfied;
    }
}
