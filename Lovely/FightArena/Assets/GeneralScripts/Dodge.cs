using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dodge : Ability
{
    private static AnimationClip[] dodgeDirections = new AnimationClip[]
    {
        _AnimationPool.GetAnimation("DodgeNeutral"),
        _AnimationPool.GetAnimation("DodgeForward"),
        _AnimationPool.GetAnimation("DodgeBackward"),
        _AnimationPool.GetAnimation("DodgeLeft"),
        _AnimationPool.GetAnimation("DodgeRight")
    };

    private DodgeDirection dodgeDirection = DodgeDirection.Neutral;
    public Dodge(Body body) : base(body)
    {
    }

    public override float Range { get { throw new System.NotImplementedException(); } }


    public void CastAbility(DodgeDirection dodgeDirection)
    {
        SetDodgeDirection(dodgeDirection);
        CastAbility();
    }
    public override void CastAbility()
    {
        performer.PlayAnimation(dodgeDirections[(int)dodgeDirection], true, false, true);
        dodgeDirection = DodgeDirection.Neutral;
    }

    public void SetDodgeDirection(DodgeDirection dodgeDirection)
    {
        this.dodgeDirection = dodgeDirection;
    }

    public override ProgressStatus CheckStatus()
    {
        throw new System.NotImplementedException();
    }

    public enum DodgeDirection
    {
        Neutral = 0,
        Forward,
        Backward,
        Left,
        Right
    }
}
