using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChasePerformable : Performable
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

    
    public readonly ISpawnable target;
    public float stoppingDistance = 0;
    float TotalStoppingDist { get { return Performer.Body.NavAgent.radius + 1 /* + target.radius*/ + stoppingDistance + 0.1f; } }

    public ChasePerformable(Mind performer, ISpawnable target)
    {
        this.Performer = performer;
        this.target = target;
    }

    public override IEnumerator Perform()
    {
        while (true)
        {
            var v = target.GameObject;
            if (target.GameObject != null && Performer.Body != null && Vector3.SqrMagnitude(target.GameObject.transform.position - Performer.Body.transform.position) > Mathf.Pow(TotalStoppingDist, 2))
            { }
            else break;

            Performer.Body.MoveToDestination(target.GameObject.transform.position);
            yield return null;
            yield return null;
            yield return null;
        }
        yield break;
    }
}
