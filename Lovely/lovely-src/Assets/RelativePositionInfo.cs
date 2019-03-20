using UnityEngine;

public class RelativePositionInfo : System.IComparable<RelativePositionInfo>
{
    readonly Vector3 _requesterPosition;
    readonly Quaternion _requesterRotation;
    readonly Vector3 _subjectPosition;
    readonly float _distance;
    readonly float _angle;

    public Vector3 requesterPosition { get { return _requesterPosition; } }
    public Quaternion requesterRotation { get { return _requesterRotation; } }
    public Vector3 subjectPosition { get { return _subjectPosition; } }
    public float distance { get { return _distance; } }
    public float angle { get { return _angle; } }
    

    public RelativePositionInfo(Vector3 requesterPosition, Quaternion requesterRotation, Vector3 subjectPosition)
    {
        _requesterPosition = requesterPosition;
        _requesterRotation = requesterRotation;
        _subjectPosition = subjectPosition;

        _distance = Vector3.Distance(requesterPosition, subjectPosition);
        var up = requesterRotation * Vector3.up;
        var forward = requesterRotation * Vector3.forward;
        var towardSubject = subjectPosition - requesterPosition;
        _angle = Vector3.SignedAngle(forward, towardSubject , up);
    }

    public int CompareTo(RelativePositionInfo other)
    {
        //if this is valid and other is not valid, this is greater
        if (other == null) return 1;
        return distance.CompareTo(other.distance);
    }
}

public class RelativePositionInfo<T> : RelativePositionInfo, System.IComparable<RelativePositionInfo<T>> where T : MonoBehaviour
{
    protected readonly T _subject;
    public T subject { get { return _subject; } }

    public RelativePositionInfo(Vector3 requesterPosition, Quaternion requesterRotation, T subject) : base(requesterPosition, requesterRotation, subject.transform.position)
    {
        _subject = subject;
    }

    public int CompareTo(RelativePositionInfo<T> other)
    {
        return base.CompareTo(other);
    }
}