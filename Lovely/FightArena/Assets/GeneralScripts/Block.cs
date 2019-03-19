using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Block : Ability
{
    private static readonly AnimationClip blockAnimation = _AnimationPool.GetAnimation("Block");

    private UnifiedController.PlayToken currentToken;

    public Block(Body body) : base(body)
    {

    }
    
    public override float Range
    {
        get
        {
            throw new System.NotImplementedException();
        }
    }

    public override void CastAbility()
    {
        if(currentToken == null || !currentToken.FrameByFrameRemainInState())
        {
            //increase mass to simulate poise then set remainonnavmesh to false so physics is received?
            currentToken = performer.PlayAnimation(blockAnimation, true, false, false);
            if(currentToken != null)
                currentToken.FrameByFrameRemainInState();
        }
    }

    public override ProgressStatus CheckStatus()
    {
        throw new System.NotImplementedException();
    }
}