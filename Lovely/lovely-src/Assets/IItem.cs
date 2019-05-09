using System;
using System.Collections;
using System.Collections.Generic;


public interface IItem : IItemStats
{
    void UseItem();
    IItem GetEmpty();
    IItem TakeAll();
}
public interface IItemStats
{
    float Value { get; }
    float ValuePerVolume { get; }
    Type ItemType { get; }
    float Volume { get; }
    bool IsEmpty { get; }
    MatterPhase Phase { get; }
}

/*
public interface IContainableItem : IItem
{ 
    //get item preview
}*/

public interface IDivisibleItem<T> : IItem where T : IItem
{
    void Combine<TOther>(ref TOther other) where TOther : T;
    bool TakeVolumeFrom<TOther>(ref TOther other, float maxTransfer) where TOther : T;
    bool UseVolume(float volume);
}
/*
public interface ICombineableItem : IItem
{
    bool Combine( ref ICombineableItem other, out IItem result);
}
*/

public interface ISpawnedItem<out T> : ISpawnable, IItemsProvider<T> where T : IItem { }

//should be used for all potentially provided types. This gives you items that you put into your inventory
//i.e. public class Foo : IItemProvider<IFood>, IItemProvider<IFuel>, ...etc 
public interface IItemsProvider<out T> : IInteractable<AcquireItemPerformable>, IBounded where T : IItem
{
    /// <summary>
    /// multiple types are expected to be implemented so this captures all
    /// </summary>
    IEnumerable<Type> ItemTypes { get; }
    bool HasItems { get; }
    bool Acquire<TAcquisitioner>(TAcquisitioner acquisitioner, out List<IItem> acquiredItems, out List<ISpawnedItem<IItem>> spawnedResources, bool requestSuccessOverride = false);
}
public interface IResource : IItem { }
public interface ITool : IResource { }
public interface IFood : IResource { }
public interface IFuel : IResource { }
public interface IPreciousMaterial : IResource { }
public interface IConstructionMaterial : IResource { }


public interface IStone : IConstructionMaterial { }
public interface IWood : IFuel, IConstructionMaterial { }
public interface ILeaves : IFuel { }






// item type determines the purpose of materials. Should be interfaces
//crafting materials are materials used in building other things. 
//construction material are materials used specifically in building structures
//precious materials are materials whos value comes from themselves, they arnt necessarily used. This is precious metal, art, gems, fine wine and the like
//tools are objects to serve a purpose. This can be creating, destroying or anything in between. Probably a toolPurpose enum will futher disabiguate
//food is anything consumed for continued survival
//fuel is any stored energy substance where the energy can be directed by the user

/*
 * 
 * 
 * Metal
 * Wood
 * Stone
 * fiber
 * cloth
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * crafting recipies list out material by interface, thus anything imlementing that interface 
 * will suffice. This allows them to be as specific as possible. For example, a recipie could require
 * an ICarveable which would be anything carveable, or an IWood which would only accept wood,
 * not stone or clay. an IHardwood would only accept a wood that is a hardwood
 * 
 * Each Item all the way from raw material to finished Product has a method GetRecipie() 
 * that returns other items and tools that can be used to create the item
 * 
 * items have a consumeQuantity on use that consumes an ammount of the item, or durability of item
 * 
 * 
 * 
 */
