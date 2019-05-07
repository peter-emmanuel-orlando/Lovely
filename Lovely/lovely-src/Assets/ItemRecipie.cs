using System;
using System.Collections.Generic;

public abstract class ItemRecipie
{
    protected abstract Dictionary<Type, float> RequiredItemVolumes { get; }
    public bool HasNeccessaryItemsToCraft(Container items)
    {
        var result = true;
        foreach (var kvPair in RequiredItemVolumes)
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