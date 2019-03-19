using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyIntel : Intel<Body>, System.IComparable<BodyIntel>//BeingIntel
{
    readonly Relationship _relationship;
    
    public Relationship relationship { get { return _relationship; } }

    public BodyIntel(Body requester, Body subject) : base(requester, subject)
    {
        _relationship = requester.Mind.GetRelationship(subject.Mind);
    }

    public int CompareTo(BodyIntel other)
    {
        return base.CompareTo(other);
    }
}