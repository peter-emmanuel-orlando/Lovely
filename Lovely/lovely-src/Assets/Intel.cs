using UnityEngine;
using System.Collections.Generic;


public class Intel<T> : RelativePositionInfo<T>, System.IComparable<Intel<T>> where T : IBounded
{
    readonly Body _requester;
    readonly bool _isVisible;

    public Body Requester { get { return _requester; } }
    public bool IsVisible { get { return _isVisible; } }

    //needs to go from the beings cameraBone
    public Intel(Body requester, T subject) : base(requester.transform.position, requester.transform.rotation, subject)
    {
        _requester = requester;
        _isVisible = requester.Mind.IsVisible(this);
    }
    

    public int CompareTo(Intel<T> other)
    {
        return base.CompareTo(other);
    }
}


/*
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
*/
