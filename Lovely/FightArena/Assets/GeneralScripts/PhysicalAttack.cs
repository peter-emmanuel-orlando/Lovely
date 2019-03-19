using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalAttack : Ability
{
    float deltaHealth;


    public PhysicalAttack(Body body) : base( body)
    {
        body.UpdateEvent += Update;
        body.TriggerEnterEvent += TriggerEnter;
    }

    public override float Range
    {
        get
        {
            throw new System.NotImplementedException();
        }
    }

    private void TriggerEnter(object sender, TriggerEventArgs e)
    {

    }

    private void Update(object sender, EventArgs e)
    {

    }

    public override IEnumerator<ProgressStatus> CastAbility()
    {
        Debug.Log("firk");
        body.PlayAnimation(_AnimationPool.GetAnimation("strike"),0,1,false);
        //throw new System.NotImplementedException();
        return null;
    }

    public override ProgressStatus CheckStatus()
    {
        throw new System.NotImplementedException();
    }
    
}
