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
    private Vector3 destination;
    float setNext = 0;

    public WanderPerformable(Mind performer)
    {
        this.performer = performer;
        destination = transform.position;
    }

    public IEnumerator Perform()
    {
        while(true)
        {
            if (Time.time > setNext )
            {
                setNext = Time.time + Random.Range(1f, 3f);
                var randomDelta = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f));
                //NavMeshHit hit;
                //NavMesh.Raycast(transform.position, transform.position + randomDelta, out hit, NavMesh.AllAreas);
                destination = transform.position + randomDelta;
            }


            Debug.DrawLine(transform.position, NavAgent.destination, Color.green, 1);
            performer.Body.MoveToDestination(destination);// hit.position;
            yield return null;
        }
    }

 

}
