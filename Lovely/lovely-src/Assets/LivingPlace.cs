using UnityEngine;
using System.Collections.Generic;
public class LivingPlace : Place
{
    readonly Container foodHolder = new Container(100, delegate(IItem item) { return item.GetType().IsAssignableFrom(typeof(IFood)); });

    protected override void Awake()
    {
        base.Awake();
    }


    public bool GetMaintinenceAssignment(Body being, ref IPerformable assignment)
    {
        var assignedPerformable = false;
        if(foodHolder.FilledVolume < 0.75f * foodHolder.MaxHoldableVolume)
        {
            assignedPerformable = true;
            assignment = GetRefillPantryDecisions(being);
        }
        return assignedPerformable;
        // storage gameobject
        // types to search for


        // acqire the resource
        //bring it back to storage
    }


    IPerformable GetRefillPantryDecisions(Body body)
    {
        IPerformable assignment = body.Mind.CurrentPerformable;
        //if beings personal storage has below a certain ammount of desired resource, forage for resource
        //if their personal storage is full, come back
        //need to check if being is already doing this
        if (body.Backpack != null && body.Backpack.FreeVolume > body.Backpack.MaxHoldableVolume * 0.9f)
            assignment = new ForageForResourcesPerformable(body.Mind, typeof(IFood));
        else
            assignment = new MoveToDestinationPerformable(body.Mind, null, transform.position, true);

        return assignment;
    }
}
