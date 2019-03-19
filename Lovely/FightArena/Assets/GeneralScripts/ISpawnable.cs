using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISpawnable
{
    string PrefabName { get; }
    GameObject gameObject { get; }
}
