using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Block : Ability
{
    private static readonly AnimationClip blockAnimation = _AnimationPool.GetAnimation("Block");

    private UnifiedController.PlayToken blockToken;
    private ProgressStatus status = ProgressStatus.Complete;

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
        if(blockToken == null || !blockToken.FrameByFrameRemainInState())
        {
            status = ProgressStatus.Complete;
            //increase mass to simulate poise then set remainonnavmesh to false so physics is received?
            blockToken = performer.PlayAnimation(blockAnimation, true, false, false);
            if(blockToken != null)
            {
                status = ProgressStatus.InProgress;
                blockToken.FrameByFrameRemainInState();
            }
        }
    }
    
    public override ProgressStatus CheckStatus()
    {
        return status;
    }
}