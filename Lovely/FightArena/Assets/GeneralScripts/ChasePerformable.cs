using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChasePerformable : IPerformable
{
    public readonly Mind performer;
    public readonly ISpawnable target;
    public float stoppingDistance = 0;
    float TotalStoppingDist { get { return performer.Body.NavAgent.radius + 1 /* + target.radius*/ + stoppingDistance + 0.1f; } }

    public ChasePerformable(Mind performer, ISpawnable target)
    {
        this.performer = performer;
        this.target = target;
    }

    public Mind Performer { get { return performer; } }

    public IEnumerator Perform()
    {
        while (true)
        {
            var v = target.GameObject;
            if (target.GameObject != null && performer.Body != null && Vector3.SqrMagnitude(target.GameObject.transform.position - performer.Body.transform.position) > Mathf.Pow(TotalStoppingDist, 2))
            { }
            else break;

            performer.Body.MoveToDestination(target.GameObject.transform.position);
            yield return null;
            yield return null;
            yield return null;
        }
        yield break;
    }
}
