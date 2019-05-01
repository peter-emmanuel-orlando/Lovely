using System;
using System.Collections.Generic;
[Serializable]
public class LivingPlaceRecord
{
    public static LivingPlaceRecord Instance { get; } = new LivingPlaceRecord();
    private static TypeDictionary<List<ISpawnable>> prefabs = new TypeDictionary< List<ISpawnable>>();
    static LivingPlaceRecord()
    {
        prefabs.Add(typeof(HumanMind), new List<ISpawnable>() );
        prefabs[typeof(HumanMind)].Add(_PrefabPool.GetPrefab("hut"));
    }

    public TwoWayDictionary<Mind, LivingPlace> ResidenceRecord { get; } = new TwoWayDictionary<Mind, LivingPlace>(true);
    
    private LivingPlaceRecord()
    {
        //due to possible deserialization
        if(!Instance.ResidenceRecord.IsEmpty)
        {
            foreach (var mind in ResidenceRecord.ForwardKeys)
            {
                foreach (var livingPlace in Instance.ResidenceRecord[mind])
                {
                    Instance.ResidenceRecord.Add(mind, livingPlace);
                }
            }
        }
    }

}
