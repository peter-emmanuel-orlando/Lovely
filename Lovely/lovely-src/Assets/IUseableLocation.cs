using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// these are places that can ba accessed and contain many Interactive locations 
/// </summary>
public interface ILocation : IBounded, IRestrictedAccess
{

}
public interface IBuildable : ILocation, ISpawnable
{
    IConstructionInfo constructionInfo { get; }
}
public interface IUseableLocation : ILocation
{
    //list<InteractiveLocations>
    bool AcquireUse<T>(out ILocationUseToken<IUseableLocation> useToken);
}
public interface ICreationLocation : IUseableLocation { }//any place you use to make other stuff, like a workbench or a cauldron or a stove

//certain recipies may require being in a specific location. This token is added to the recipie as if it were an item
// all it does is check if Holders current bounds overlaps its location
public interface ILocationUseToken<T> where T : IUseableLocation
{
    IBounded Holder { get; }
    T Location { get; }
    bool IsValid { get; }
}
public interface IInteractable<out T> where T : IPerformable
{
    T GetInteractionPerformable(Body performer);

    //IEnumerable<IInteractiveLocation> InteractiveLocations { get; }
    //IInteractiveLocation GetClosestInteractiveLocations { get; }
}

/// <summary>
/// represents one single location that when occupied, allows interaction with an interactable. An interactable can have many interactiveLocations
/// </summary>
public interface IInteractiveLocation : ILocation
{
    //IInteractable<IPerformable> interactable { get; }
}

public class InteractiveLocation : MonoBehaviour, IInteractiveLocation
{
    public Bounds Bounds => throw new System.NotImplementedException();

    public bool IsAuthorized(IAuthorizationToken<IUseableLocation> authToken)
    {
        return true;
    }
}

