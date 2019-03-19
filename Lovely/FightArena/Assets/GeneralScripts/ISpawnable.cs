using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//maybe rename to GameEntity
public interface ISpawnable
{
    string PrefabName { get; }
    GameObject gameObject { get; }
    Transform transform { get; }
}
