using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComittedAnimation
{
    public readonly AnimationClip clip;
    public readonly float lockInAnimStart;
    public readonly float lockInAnimEnd;

    public ComittedAnimation(AnimationClip clip, float lockInAnimStart, float lockInAnimEnd)
    {
        this.clip = clip;
        this.lockInAnimStart = lockInAnimStart;
        this.lockInAnimEnd = lockInAnimEnd;
    }
}
