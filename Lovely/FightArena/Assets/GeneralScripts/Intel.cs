using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Intel  : IComparable<Intel>, IEquatable<Intel>
{
    public readonly bool isInitialized;
    public readonly float timeStamp;
    public readonly GameObject observer;
    public readonly ISpawnable subject;
    public readonly float distance;

    public Intel(GameObject observer, ISpawnable subject)
    {
        isInitialized = true;
        timeStamp = Time.time;
        this.observer = observer;
        this.subject = subject;
        distance = Vector3.Distance(observer.transform.position, subject.GameObject.transform.position);
    }
    

    

    public int CompareTo(Intel other)
    {
        return this.distance.CompareTo(other.distance);
    }

    public bool Equals(Intel other)
    {
        var result =
            isInitialized == other.isInitialized &&
            timeStamp == other.timeStamp &&
            observer == other.observer &&
            subject == other.subject &&
            distance == other.distance;

        return result;
    }
}
