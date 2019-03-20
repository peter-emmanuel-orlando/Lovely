using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Relationship
{
    private float friendshipLevel;//-1 to 1
    private float superiorityLevel;//-1 to 1 
    //fightOrFlight only if friendship < 0;

    public bool prey;
    public bool predator;

    public float FriendshipLevel
    {
        get
        {
            return friendshipLevel;
        }

        set
        {
            friendshipLevel = Mathf.Clamp(value, -1, 1); ;
        }
    }

    public float SuperiorityLevel
    {
        get
        {
            return superiorityLevel;
        }

        set
        {
            superiorityLevel = Mathf.Clamp(value, -1, 1); ;
        }
    }



    public Relationship(float friendshipLevel, float superiorityLevel, bool prey, bool predator )
    {
        this.friendshipLevel = friendshipLevel;
        this.superiorityLevel = superiorityLevel;
        this.prey = prey;
        this.predator = predator;
    }
}
