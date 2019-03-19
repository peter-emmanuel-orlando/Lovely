using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyPerformable : IPerformable
{
    public static readonly IPerformable empty = new EmptyPerformable();

    public Mind Performer { get { return null; } }

    public IEnumerator Perform()
    {
        yield break;
    }
}
