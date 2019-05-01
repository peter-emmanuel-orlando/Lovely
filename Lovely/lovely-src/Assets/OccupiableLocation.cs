using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class OccupiableLocation : MonoBehaviour, IBounded
{
    /// 
    /// this is a "move your feet, loose your seat" sort of location. A being occupies it simply by standing in it. This is opposed to an assignable location
    /// Assignable locations can be occupied by other beings that happen to be wandering through, but the assignable location will query weather they are
    /// currently performing a task, if they arnt, it'll assign them a task of "move somewhere else".
    /// This class is for sensing, meaning it doesnt enforce maxOccupiers
    /// maybe it should be a navmesharea to dissuade pathfinding through it. requesting a spot adds the area to  navAgent.AreaMask
    ///


    [SerializeField]
    protected int maxOccupants = 1;
    [SerializeField]
    List<Body> currentOccupiers = new List<Body>(); // make hashsett


    public Bounds Bounds { get => throw new System.NotImplementedException(); }
    public virtual bool HasVacancy { get { return currentOccupiers.Count < maxOccupants; } }
    public bool IsFull { get { return !HasVacancy; } }

    protected virtual void Awake()
    {
        var potentialZones = GetComponentsInChildren<Collider>();

        List<Collider> inhabitableZones = new List<Collider>();
        foreach (var col in potentialZones)
        {
            if (col != null && col.isTrigger && col.gameObject.layer == LayerMask.NameToLayer("HitHurtBoxSensing"))
                inhabitableZones.Add(col);
        }

        if(inhabitableZones.Count == 0)
            Debug.LogWarning("for gameObject '" + gameObject + "' there is no collider attached to this Occupiable Location, nothing will ever occupy it!\nOnly colliders that are on the 'HitHurtBoxSensing' layer and are a trigger qualify");

            var rb= GetComponent<Rigidbody>();
        if (rb == null)
           rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.Sleep();
    }
    

    public bool IsOccupant(Body being)
    {
        return currentOccupiers.Contains(being);
    }

    void OnTriggerEnter(Collider other)
    {
        Body occupier = other.GetComponentInParent<Body>();
        if(occupier != null && !currentOccupiers.Contains(occupier))
            currentOccupiers.Add(occupier);
    }

    void OnTriggerExit(Collider other)
    {
        Body occupier = other.GetComponentInParent<Body>();
        if (occupier != null && currentOccupiers.Contains(occupier))
            currentOccupiers.Remove(occupier);
    }
}
