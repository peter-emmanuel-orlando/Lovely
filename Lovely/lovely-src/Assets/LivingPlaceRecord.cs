using System;
using System.Collections.Generic;
[Serializable]
public class LivingPlaceRecord
{
    public static LivingPlaceRecord Instance { get; } = new LivingPlaceRecord();
    public static Dictionary<Mind, HashSet<LivingPlace>> prefabs = new Dictionary<Mind, HashSet<LivingPlace>>();
    static LivingPlaceRecord()
    {
        _PrefabPool.GetPrefab()
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
