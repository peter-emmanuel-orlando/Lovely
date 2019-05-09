using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public static class LivingPlacePrefabs
{
    private static List<Record> records = new List<Record>();

    static LivingPlacePrefabs()
    {
        records.Add(new Record(o => o is HumanoidBody, _PrefabPool.GetPrefab("Hut")));
    }

    private class Record
    {
        public Func<object, bool> QualifiesToBuild { get; }
        public ISpawnable prefab { get; }

        public Record(Func<object, bool> qualifiesToBuild, ISpawnable prefab)
        {
            this.QualifiesToBuild = qualifiesToBuild ?? throw new ArgumentNullException(nameof(qualifiesToBuild));
            this.prefab = prefab ?? throw new ArgumentNullException(nameof(prefab));
        }
    }
}
