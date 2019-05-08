using System.Collections.Generic;
using UnityEngine;


//create interface too 

public interface IInteractable<out T> where T : IPerformable
{
    T GetInteractionPerformable(Body performer);

    //IEnumerable<IInteractiveLocation> InteractiveLocations { get; }
    //IInteractiveLocation GetClosestInteractiveLocations { get; }
}

/// <summary>
/// represents one single location that when occupied, allows interaction with an interactable. An interactable can have many interactiveLocations
/// </summary>
public interface IInteractiveLocation : IBounded
{}

public class InteractiveLocation : MonoBehaviour, IInteractiveLocation
{
    public Bounds Bounds => throw new System.NotImplementedException();

}