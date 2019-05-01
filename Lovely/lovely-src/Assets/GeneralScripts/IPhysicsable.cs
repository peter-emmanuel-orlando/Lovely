using System;
using System.Collections.Generic;
using UnityEngine;

public interface IPhysicsable
{
    void AddForce(Vector3 force, ForceMode forceMode);
}

public class Physicsable : IPhysicsable, IEquatable<Physicsable>
{
    public Physicsable(Rigidbody rb)
    {
        this.rb = rb ?? throw new ArgumentNullException(nameof(rb));
    }

    private Rigidbody rb { get; }
    public void AddForce(Vector3 force, ForceMode forceMode)
    {
        rb.AddForce(force, forceMode);
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Physicsable);
    }

    public bool Equals(Physicsable other)
    {
        return other != null &&
               EqualityComparer<Rigidbody>.Default.Equals(rb, other.rb);
    }

    public override int GetHashCode()
    {
        return EqualityComparer<Rigidbody>.Default.GetHashCode(rb);
    }

    public static bool operator ==(Rigidbody left, Physicsable right)
    {
        return EqualityComparer<Rigidbody>.Default.Equals(left, right.rb);
    }

    public static bool operator !=(Rigidbody left, Physicsable right)
    {
        return !(left == right);
    }

    public static bool operator ==(Physicsable left, Rigidbody right)
    {
        return EqualityComparer<Rigidbody>.Default.Equals(left.rb, right);
    }

    public static bool operator !=(Physicsable left, Rigidbody right)
    {
        return !(left == right);
    }

}
