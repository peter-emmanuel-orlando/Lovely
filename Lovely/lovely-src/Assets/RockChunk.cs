using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockChunk : SpawnedItem<StoneItem>
{

    public const string _PrefabName = "RockChunk";
    public override string PrefabName => _PrefabName;
}
