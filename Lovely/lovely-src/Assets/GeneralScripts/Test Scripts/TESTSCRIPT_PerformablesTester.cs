using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TESTSCRIPT_PerformablesTester : MonoBehaviour, IDecisionMaker
{
    [ShowOnly]
    PerceivingMind performer;
    IPerformable currentPerformable;

    [SerializeField]
    GameObject target;

    public IPerformable GetDecisions()
    {
        if(currentPerformable == null)
            currentPerformable = new FightPerformable(performer);

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
