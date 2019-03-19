using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControlDecisionMaker : IDecisionMaker
{
    PlayerControlPerformable performable;
    Mind mind;

    public PlayerControlDecisionMaker(Mind mind)
    {
        this.mind = mind;
        performable = new PlayerControlPerformable(mind);
    }

    public IPerformable GetDecisions()
    {
        return performable;
    }
}
