using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//maybe rename to GameEntity
public interface ISpawnable
{
    string PrefabName { get; }

    //needs to be nullchecked before accessing gameobject or else error
    GameObject GameObject { get; }    
    Transform Transform { get; }
    Bounds Bounds { get; }
}

public interface ISpawnableBody : ISpawnable { }
public interface ISpawnableBuilding : ISpawnable { }
public interface ISpawnableCraftingMaterial : ISpawnable { }
public interface ISpawnableConstructionMaterial : ISpawnable { }
public interface ISpawnablePreciousMaterial : ISpawnable { }
public interface ISpawnableTool : ISpawnable { }
public interface ISpawnableFood : ISpawnable { }
public interface ISpawnableFuel : ISpawnable { }

/*
    None =                  0,
    CraftingMaterial =      1 << 0,
    ConstructionMaterial =  1 << 1,
    PreciousMaterial =      1 << 2,
    Tool =                  1 << 3,
    Food =                  1 << 4,
    Fuel =                  1 << 5
 */
