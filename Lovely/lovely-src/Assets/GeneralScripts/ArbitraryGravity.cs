using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ArbitraryGravity : MonoBehaviour, IBounded
{
    [SerializeField]
    public Bounds Bounds { get; private set; }
    [ShowOnly]
    [SerializeField]
    List<IPhysicsable> effectedObjects = new List<IPhysicsable>();
    private void Awake()
    {
        Bounds = new Bounds(transform.position, Vector3.one * float.PositiveInfinity);
        TrackedComponent.Track(this);
    }

    private void FixedUpdate()
    {
        foreach (var physicsable in effectedObjects)
        {            
            ApplyGravity(physicsable);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var physable = other.GetComponentInChildren<IPhysicsable>();
        if (physable != null)
            effectedObjects.Add(physable);
        else if (other.attachedRigidbody != null)
            effectedObjects.Add(new Physicsable(other.attachedRigidbody));
    }
    private void OnTriggerExit(Collider other)
    {
        var physable = other.GetComponentInChildren<IPhysicsable>();
        if (physable != null)
            effectedObjects.Remove(physable);
        else if (other.attachedRigidbody != null)
            effectedObjects.Remove(new Physicsable(other.attachedRigidbody));
    }

    protected abstract void ApplyGravity(IPhysicsable subject);
}

public class GravityPlane : ArbitraryGravity
{
    [SerializeField]
    Vector3 localGravity = Vector3.down;

    public Vector3 LocalGravity { get { return localGravity; } }
    public Vector3 WorldGravity { get { return transform.rotation * localGravity; } }

    protected override void ApplyGravity(IPhysicsable subject)
    {
        subject.AddForce(WorldGravity, ForceMode.Acceleration);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawCube(Bounds.center, Bounds.size);
    }
}