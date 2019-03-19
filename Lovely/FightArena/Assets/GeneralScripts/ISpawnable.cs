using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISpawnable
{
    string prefabName { get; }
    GameObject gameObject { get; }
}
