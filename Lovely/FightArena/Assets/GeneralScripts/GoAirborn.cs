using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoAirborn : Ability
{
    private Vector3 jumpDirection = Vector3.up;

    public GoAirborn(Body body) : base(body)
    {
    }

    //if body is not attached to navMesh use fly, else use jump
    public override float Range
    {
        get
        {
            throw new System.NotImplementedException();
        }
    }

    public override void CastAbility()
    {

    }
    
    public override ProgressStatus CheckStatus()
    {
        throw new System.NotImplementedException();
    }
}
