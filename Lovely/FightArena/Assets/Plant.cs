using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Plant : Spawnable
{
    //plants spread automatically within a certain distance, simulating seeds or budding or whatever
    protected abstract void Reproduce(); 
}
