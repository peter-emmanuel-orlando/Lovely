using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class HumanMind : SurvivalMind
{
    public HumanMind(Body body) : base(body)
    {
        WakefulnessMax = 1;
        ExcitementMax = 5;
        SpiritualityMax = 3;
        SocializationMax = 2;
    }
}
