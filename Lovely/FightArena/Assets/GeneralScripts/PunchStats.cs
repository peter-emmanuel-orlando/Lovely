using UnityEngine;

public partial class PunchCombo
{
    private struct PunchStats
    {
        public readonly AnimationClip punchAnimation;
        public readonly bool isMirrored;
        public readonly HitBoxType hitBoxType;
        public readonly AnimationClip knockBackAnimation;
        public readonly float deltaHealth;

        public PunchStats(AnimationClip punchAnimation, bool isMirrored, HitBoxType hitBoxType, AnimationClip knockBackAnimation, float deltaHealth)
        {
            this.punchAnimation = punchAnimation;
            this.isMirrored = isMirrored;
            this.hitBoxType = hitBoxType;
            this.knockBackAnimation = knockBackAnimation;
            this.deltaHealth = deltaHealth;
        }
    }
}








