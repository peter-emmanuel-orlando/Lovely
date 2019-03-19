using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class WanderPerformable : IPerformable
{
    Mind performer;
    public Mind Performer { get { return performer; } }
    private NavMeshAgent NavAgent{get{ return Performer.Body.NavAgent; }}
    private Transform transform { get { return performer.Body.transform; } }
    float setNext = 0;

    public WanderPerformable(Mind performer)
    {
        this.performer = performer;
    }

    public IEnumerator Perform()
    {
        while(true)
        {
            if (Time.time > setNext && (!NavAgent.hasPath && !NavAgent.pathPending))
            {
                setNext = Time.time + Random.Range(3f, 8f);
                var randomDelta = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f));
                //NavMeshHit hit;
                //NavMesh.Raycast(transform.position, transform.position + randomDelta, out hit, NavMesh.AllAreas);
                NavAgent.destination = transform.position + randomDelta;// hit.position;
                Debug.DrawLine(transform.position, NavAgent.destination, Color.green, 1);
            }
            yield return null;
        }
    }

 

}
