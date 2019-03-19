using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChasePerformable : IPerformable
{
    public readonly Mind performer;
    public readonly ISpawnable target;

    public ChasePerformable(Mind performer, ISpawnable target)
    {
        this.performer = performer;
        this.target = target;
    }

    public Mind Performer { get { return performer; } }

    public IEnumerator Perform()
    {
        while (Vector3.Distance(target.gameObject.transform.position, performer.Body.transform.position) > 1.5)
        {
            performer.Body.MoveToDestination(target.gameObject.transform.position);
            yield return null;
        }
        yield break;
    }
}
