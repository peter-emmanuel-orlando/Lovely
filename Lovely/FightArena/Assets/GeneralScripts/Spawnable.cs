using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawnable : MonoBehaviour, ISpawnable
{
    [SerializeField]
    string prefabName;

    public string PrefabName
    {
        get { return prefabName; }
    }
}
