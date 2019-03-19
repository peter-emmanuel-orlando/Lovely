using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Intel  : IComparer<Intel>
{
    public readonly float timeStamp;
    public readonly GameObject observer;
    public readonly ISpawnable subject;
    public readonly float distance;

    public Intel(GameObject observer, ISpawnable subject)
    {
        timeStamp = Time.time;
        this.observer = observer;
        this.subject = subject;
        distance = Vector3.Distance(observer.transform.position, subject.gameObject.transform.position);
    }
    

    int IComparer<Intel>.Compare(Intel x, Intel y)
    {
        return x.distance.CompareTo(y.distance);
    }
}
