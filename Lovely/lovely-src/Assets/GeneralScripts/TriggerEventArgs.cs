using System;
using UnityEngine;

public delegate void ColliderEventHandler(GameObject sender, ColliderEventArgs e);

public class ColliderEventArgs : EventArgs
{
    public readonly Collider collider;
    public ColliderEventArgs(Collider collider)
    { this.collider = collider; }
}
