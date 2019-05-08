using UnityEngine;

public interface IRelativePositionInfo : System.IComparable<IRelativePositionInfo>
{
    float angle { get; }
    float distance { get; }
    Vector3 requesterPosition { get; }
    Quaternion requesterRotation { get; }
    Vector3 subjectPosition { get; }
}

public class RelativePositionInfo : IRelativePositionInfo
{
    public Vector3 requesterPosition { get; }
    public Quaternion requesterRotation { get; }
    public Vector3 subjectPosition { get; }
    public float distance { get; }
    public float angle { get; }


    public RelativePositionInfo(Vector3 requesterPosition, Quaternion requesterRotation, Vector3 subjectPosition)
    {
        this.requesterPosition = requesterPosition;
        this.requesterRotation = requesterRotation;
        this.subjectPosition = subjectPosition;

        distance = Vector3.Distance(requesterPosition, subjectPosition);
        var up = requesterRotation * Vector3.up;
        var forward = requesterRotation * Vector3.forward;
        var towardSubject = subjectPosition - requesterPosition;
        angle = Vector3.SignedAngle(forward, towardSubject, up);
    }

    public int CompareTo(IRelativePositionInfo other)
    {
        //if this is valid and other is not valid, this is greater
        if (other == null) return 1;
        return distance.CompareTo(other.distance);
    }
}

public interface IRelativePositionInfo<out T> : IRelativePositionInfo where T : IBounded
{
    T Subject { get; }
}
    public class RelativePositionInfo<T> : RelativePositionInfo, IRelativePositionInfo<T> where T : IBounded
{
    public T Subject { get; protected set; }

    public RelativePositionInfo(Vector3 requesterPosition, Quaternion requesterRotation, T subject) : base(requesterPosition, requesterRotation, subject.Bounds.center)
    {
        Subject = subject;
    }
}