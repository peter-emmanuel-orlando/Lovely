using System;
using System.Collections.Generic;
[Serializable]
public static class LivingPlaceRecord
{
    //this is  List of KVPairs where k is a hashset of interfaces of interfaces that body must implement and v is a prefab.
    //you say for all in the list, if !requirements.issubsetof(providedQualifications)
    /*
    private static HashSet<> prefabs = new TypeDictionary<List<ISpawnable>>();
    static LivingPlaceRecord()
    {
        prefabs.Add(typeof(HumanMind), new List<ISpawnable>() );
        prefabs[typeof(HumanMind)].Add(_PrefabPool.GetPrefab("Hut"));
    }

    
    private LivingPlaceRecord()
    {

    }
    */
}
