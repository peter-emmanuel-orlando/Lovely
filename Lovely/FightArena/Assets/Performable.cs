using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Performable : IPerformable
{

    //change to each mindStat in in-game days. negative takes away, positive adds
    public abstract float DeltaWakefulness { get; }//decreases when awake, depletes quicker when performing taxing tasks, increases 1.5x speed when sleeping
    public abstract float DeltaExcitement { get; }//depletes awake or asleep. Tedious work takes chunks from this. relaxation activities increase this
    public abstract float DeltaSpirituality { get; }//depletes awake or asleep. relaxation activities increase this. Tedious Work or seedy activities takes chunks from this
    public abstract float DeltaSocialization { get; }//depletes when awake. increases when working or playing together
    public abstract float DeltaCalories { get; }//num days calories will last.
    public abstract float DeltaBlood { get; }//reaching 0 blood and being passes out
    public abstract bool IsSleepActivity { get; }//is this activity sleeping?




    public abstract ActivityState ActivityType { get; }

    protected Mind _performer;
	public Mind Performer{get{ return _performer; } set{ _performer = value; }}

	protected bool _isPerforming = false;
	public bool IsPerforming{get{ return _isPerforming; }}

	protected bool _isComplete = false;
	public bool IsComplete{get{ return _isComplete; }}

	//body looks at this when performable is complete to determine whether to call onSuccess or Onfaliure
	protected bool _success = true;

    protected Performable(Mind performer)
    {
        if (performer == null) throw new ArgumentNullException();
        _performer = performer;
    }

    protected Performable()
    {
    }

    public bool Success{get{return _success;}}

	public abstract IEnumerator Perform ();


	//should end everything.
	public virtual void Abort(){_isComplete = true;}

}

