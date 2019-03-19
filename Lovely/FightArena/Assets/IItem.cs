using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IItem
{
	float Volume { get; }
    ItemType type { get; }
    MatterPhase Phase { get; }
    //get item preview

}
