using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SleepPerformable : Performable
{

    //change to each mindStat per in-game day. negative takes away, positive adds
    public override float DeltaWakefulness { get { return _deltaWakefulness; } }//decreases when awake, depletes quicker when performing taxing tasks, increases 1.5x speed when sleeping
    public override float DeltaExcitement { get { return _deltaExcitement; } }//depletes awake or asleep. Tedious work takes chunks from this. relaxation activities increase this
    public override float DeltaSpirituality { get { return _deltaSpirituality; } }//depletes awake or asleep. relaxation activities increase this. Tedious Work or seedy activities takes chunks from this
    public override float DeltaSocialization { get { return _deltaSocialization; } }//depletes when awake. increases when working or playing together
    public override float DeltaCalories { get { return _deltaCalories; } }//num days calories will last.
    public override float DeltaBlood { get { return _deltaBlood; } }//reaching 0 blood and being passes out
    public override bool IsSleepActivity { get { return _isSleepActivity; } }//is unconscious. This should be a property of body
    //this is redundant because activity type will be sleep
    float _deltaWakefulness = 2f;
    float _deltaExcitement = 0;//random maybe?
    float _deltaSpirituality = 0.3f;
    float _deltaSocialization = 0;
    float _deltaCalories = 0;
    float _deltaBlood = 0;
    bool _isSleepActivity = true;
    //*//////////////////////////////////////////////////////////////////////////////////////////////////
    
    Func<bool> awakeConditions = delegate () { return true; };

    public override ActivityState ActivityType { get { return ActivityState.Rest; } }

    UnifiedController.PlayToken token = null;

    public SleepPerformable(Mind mind, Func<bool> awakeConditions ) : base(mind)
    {
        this.awakeConditions = awakeConditions;
        _isComplete = true;
    }

    public void AwakeSleeper()
    {
        if(!IsComplete)
        {
            _isComplete = true;
            if(token != null)
                token.Abort();
        }
        token = null;
    }

    public override IEnumerator Perform()
    {
        //determine if its possible to go home
        //if not look for suteable spot to sleep
        //being.anim.SetBool("Sleep", true);
        _isComplete = false;        
        var tryUntil = Time.time + 2f;
        while(Time.time < tryUntil)
        {
            token = Performer.Body.PlayAnimation(_AnimationPool.GetAnimation("HumanoidFall"));
            yield return null;
        }
        while (!awakeConditions() && !IsComplete && token != null)
        {
            if (token != null) token.FrameByFrameRemainInState();
            yield return null;
        }
        AwakeSleeper();
    }

    public override void Abort()
    {
        base.Abort();
        AwakeSleeper();
    }
}
