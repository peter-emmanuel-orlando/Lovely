using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//maybe rename to GameEntity
public interface ISpawnable : IGameObject, IBounded
{
    string PrefabName { get; }

    //needs to be nullchecked before accessing gameobject or else error
}
public interface IGameObject
{

    //needs to be nullchecked before accessing gameobject or else error
    GameObject GameObject { get; }
    Transform Transform { get; }
}
