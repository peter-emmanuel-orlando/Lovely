using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TESTSCRIPT_PerformablesTester : MonoBehaviour, IDecisionMaker
{
    [ShowOnly]
    Mind performer;
    ChasePerformable currentPerformable;

    [SerializeField]
    GameObject target;

    public IPerformable GetDecisions()
    {
        var spwn = target.GetComponentInChildren<ISpawnable>();
        if(spwn != null)
        {
            currentPerformable = new ChasePerformable(performer, spwn);
        }

        return currentPerformable;
    }

    // Use this for initialization
    void Start ()
    {
        performer = GetComponent<Body>().Mind;
        performer.OverrideDecisionMaker(this);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
