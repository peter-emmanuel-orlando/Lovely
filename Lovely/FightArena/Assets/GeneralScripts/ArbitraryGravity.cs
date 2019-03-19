using System.Collections;
using UnityEngine;

public abstract class ArbitraryGravity : TrackedGameObject<ArbitraryGravity>
{
    static ArbitraryGravity()
    {
        LocUpdatesPerFrame = 1;
        CellSize = 100;
    }
}

public class GravityPlane : ArbitraryGravity
{
    [SerializeField]
    Vector3 localGravity = Vector3.down;

    public Vector3 LocalGravity { get { return localGravity; } }
    public Vector3 WorldGravity { get { return transform.rotation * localGravity; } }


    private void OnTriggerEnter(Collider other)
    {

    }

    private void OnDrawGizmosSelected()
    {

    }

    protected override ArbitraryGravity This { get { return this; } }

    protected override Bounds Bounds { get { throw new System.NotImplementedException(); } }
}