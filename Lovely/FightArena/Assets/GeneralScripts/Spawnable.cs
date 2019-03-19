using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawnable : MonoBehaviour, ISpawnable
{
    public string PrefabName
    {
        get { return gameObject.name; }
    }
}
