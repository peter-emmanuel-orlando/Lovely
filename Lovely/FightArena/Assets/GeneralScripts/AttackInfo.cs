using UnityEngine;

public partial class ComboAbility
{
    private struct AttackInfo
    {
        public readonly ScheduledMessageAnimation attackAnimation;
        public readonly bool isMirrored;
        public readonly HitBoxType hitBoxType;
        public readonly ScheduledMessageAnimation knockBackAnimation;
        public readonly float deltaHealth;

        public AttackInfo(ScheduledMessageAnimation punchAnimation, bool isMirrored, HitBoxType hitBoxType, ScheduledMessageAnimation knockBackAnimation, float deltaHealth)
        {
            this.attackAnimation = punchAnimation;
            this.isMirrored = isMirrored;
            this.hitBoxType = hitBoxType;
            this.knockBackAnimation = knockBackAnimation;
            this.deltaHealth = deltaHealth;
        }
    }
}








