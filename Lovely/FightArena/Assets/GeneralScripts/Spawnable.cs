using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawnable : MonoBehaviour, ISpawnable
{
    public string PrefabName
    {
        get { return gameObject.name; }
    }

    public GameObject GameObject { get { return (this == null) ? null : base.gameObject; } }
    public Transform Transform { get { return (this == null) ? null : transform; } }
}
