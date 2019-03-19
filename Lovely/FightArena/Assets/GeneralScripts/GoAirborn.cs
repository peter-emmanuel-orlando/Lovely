using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GainAltitude : Ability
{
    private Vector3 jumpDirection = Vector3.up;

    public GainAltitude(Body body) : base(body)
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

    //glide does a few things...
    //      (1) effectively decreases gravity by adding a percentage of the downward force of gravity
    //      in the opposite direction as gravity. (moderated by angle of climb [azimuth])
    //
    //      (2) redirect momentum
    //
    //      (3) increase air resistance perpendicular to body plane, zero parallel

    public void SetMovementDirection()
    {

    }

    public override void CastAbility()
    {
        /*
        normalizedJump = new Vector3(normalizedJump.x * jumpVelocity.x, normalizedJump.y * jumpVelocity.y, normalizedJump.z * jumpVelocity.z);

        //do i want this to scale with creature? thats what transform vector does
        var modifiedVelocity = transform.TransformVector(normalizedJump);
        AddForce(modifiedVelocity, ForceMode.VelocityChange, 0.3f);
            */
    }
    
    public override ProgressStatus CheckStatus()
    {
        throw new System.NotImplementedException();
    }
}
