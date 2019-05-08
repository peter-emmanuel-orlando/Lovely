using UnityEngine;
using System.Collections.Generic;

public interface IIntel< out T> : IRelativePositionInfo<T> where T : IBounded
{
    bool IsVisible { get; }
    Body Requester { get; }
}

public class Intel<T> : RelativePositionInfo<T>, IIntel<T> where T : IBounded
{
    public Body Requester { get; }
    public bool IsVisible { get; }
    public Intel(Body requester, T subject) : base(requester.transform.position, requester.transform.rotation, subject)
    {
        Requester = requester;
        IsVisible = requester.Mind.IsVisible(this);
    }
}
