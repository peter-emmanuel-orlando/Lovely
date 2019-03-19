using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dodge : Ability
{
    private static readonly AnimationClip[] genericDodgeAnimations = new AnimationClip[]
    {
        _AnimationPool.GetAnimation("Dodge_N_M"),
        _AnimationPool.GetAnimation("Dodge_F_M"),
        _AnimationPool.GetAnimation("Dodge_B_M"),
        _AnimationPool.GetAnimation("Dodge_L_M"),
        _AnimationPool.GetAnimation("Dodge_R_M")
    };


    private readonly AnimationClip[] dodgeAnimations;

    UnifiedController.PlayToken current = null;

    private DodgeDirection dodgeDirection = DodgeDirection.Neutral;
    public Dodge(Body body) : base(body)
    {
        dodgeAnimations = genericDodgeAnimations;
    }
    public Dodge(Body body, AnimationClip neutralDodge, AnimationClip forwardDodge, AnimationClip backwardDodge, AnimationClip leftDodge, AnimationClip rightDodge ) : base(body)
    {
        if (!neutralDodge) neutralDodge = genericDodgeAnimations[0];
        if (!forwardDodge) forwardDodge = genericDodgeAnimations[1];
        if (!backwardDodge) backwardDodge = genericDodgeAnimations[2];
        if (!leftDodge) leftDodge = genericDodgeAnimations[3];
        if (!rightDodge) rightDodge = genericDodgeAnimations[4];

        dodgeAnimations = new AnimationClip[] { neutralDodge, forwardDodge, backwardDodge, leftDodge, rightDodge };
    }

    public override float Range { get { throw new System.NotImplementedException(); } }


    public void CastAbility(Vector3 dodgeDirection)
    {
        ChooseBestDodgeDirection(dodgeDirection);
        CastAbility();
    }
    public void CastAbility(DodgeDirection dodgeDirection)
    {
        SetDodgeDirection(dodgeDirection);
        CastAbility();
    }
    public override void CastAbility()
    {
        var block = performer.CharacterAbilities[CharacterAbilitySlot.Block];
        if(current == null || current.GetProgress() == -1/* || current.GetProgress() > 0.8f*/)
        {
            //commented out section gives dodge ability to overlap itself. looses impact of animation, but is more responsive
            if ((block != null && block.CheckStatus() == ProgressStatus.InProgress)/* || (current != null && current.GetProgress() > 0.9f) */)
                current = performer.PlayInterruptAnimation(dodgeAnimations[(int)dodgeDirection], true, false, true);//Interrupt
            else
                current = performer.PlayAnimation(dodgeAnimations[(int)dodgeDirection], true, false, true);// dont Interrupt if not in block

            dodgeDirection = DodgeDirection.Neutral;
        }
    }

    public void SetDodgeDirection(DodgeDirection dodgeDirection)
    {
        this.dodgeDirection = dodgeDirection;
    }

    public void ChooseBestDodgeDirection(Vector3 localVec)
    {
        //Debug.Log(localVec);
        localVec = localVec.normalized;
        //Debug.Log(localVec);
        var newDirection = DodgeDirection.Neutral;

        if(localVec != Vector3.zero && localVec != Vector3.up)
        {
            var currentMax = 0f;
            var current = 0f;
            var debug = "";
            if ((current = Vector3.Dot(localVec, Vector3.forward)) > currentMax)
            {
                newDirection = DodgeDirection.Forward;
                currentMax = current;
            }
            debug += "f:" + current + ", ";

            if ((current = Vector3.Dot(localVec, Vector3.back)) > currentMax)
            {
                newDirection = DodgeDirection.Backward;
                currentMax = current;
            }
            debug += "b:" + current + ", ";

            if ((current = Vector3.Dot(localVec, Vector3.left)) > currentMax)
            {
                newDirection = DodgeDirection.Left;
                currentMax = current;
            }
            debug += "l:" + current + ", ";

            if ((current = Vector3.Dot(localVec, Vector3.right)) > currentMax)
            {
                newDirection = DodgeDirection.Right;
                currentMax = current;
            }
            debug += "r:" + current + ", ";

            debug += "max:" + currentMax;
            //Debug.Log(debug);
        }

        SetDodgeDirection(newDirection);
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
