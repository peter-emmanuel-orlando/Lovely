using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class WanderPerformable : Performable
{

    //change to each mindStat in in-game days. negative takes away, positive adds
    public override float DeltaWakefulness { get { return _deltaWakefulness; } }//decreases when awake, depletes quicker when performing taxing tasks, increases 1.5x speed when sleeping
    public override float DeltaExcitement { get { return _deltaExcitement; } }//depletes awake or asleep. Tedious work takes chunks from this. relaxation activities increase this
    public override float DeltaSpirituality { get { return _deltaSpirituality; } }//depletes awake or asleep. relaxation activities increase this. Tedious Work or seedy activities takes chunks from this
    public override float DeltaSocialization { get { return _deltaSocialization; } }//depletes when awake. increases when working or playing together
    public override float DeltaCalories { get { return _deltaCalories; } }//num days calories will last.
    public override float DeltaBlood { get { return _deltaBlood; } }//reaching 0 blood and being passes out
    public override bool IsSleepActivity { get { return _isSleepActivity; } }//is this activity sleeping?

    float _deltaWakefulness = 0;
    float _deltaExcitement = 0;
    float _deltaSpirituality = 0;
    float _deltaSocialization = 0;
    float _deltaCalories = 0;
    float _deltaBlood = 0;
    bool _isSleepActivity = false;
    //*//////////////////////////////////////////////////////////////////////////////////////////////////

    public override ActivityState ActivityType { get { return ActivityState.Nothing; } }

    private NavMeshAgent NavAgent{get{ return Performer.Body.NavAgent; }}
    private Transform Transform { get { return Performer.Body.transform; } }
    private Vector3 destination;
    float setNext = 0;

    public WanderPerformable(PerceivingMind performer)
    {
        this._performer = performer;
        destination = Transform.position;
    }

    public override IEnumerator Perform()
    {
        while(true)
        {
            if (Time.time > setNext )
            {
                setNext = Time.time + Random.Range(1f, 3f);
                var randomDelta = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f));
                NavMesh.Raycast(Transform.position, Transform.position + randomDelta, out NavMeshHit hit, NavMesh.AllAreas);
                destination = hit.position;
            }


            Debug.DrawLine(Transform.position, NavAgent.destination, Color.green, 1);
            Performer.Body.MoveToDestination(destination);// hit.position;
            yield return null;
        }
    }

 

}
