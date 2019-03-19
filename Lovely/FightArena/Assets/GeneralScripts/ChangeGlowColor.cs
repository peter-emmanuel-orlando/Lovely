using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeGlowColor : RestrictedPerformable<IBodyCanGlow>
{
    public readonly Color finalColor;
    public readonly float duration;

    public ChangeGlowColor(Color finalColor, float duration, Body body, IBodyCanGlow extraFeature) : base(body, extraFeature)
    {
        this.finalColor = finalColor;
        this.duration = duration;
    }    

    public override IEnumerator Perform()
    {
        if (duration <= 0)
        {
            extraFeature.GlowColor = finalColor;
        }
        else
        {
            var elapsedTime = 0f;
            var originalColor = extraFeature.GlowColor;
            while (elapsedTime <= duration)
            {
                var lerpFactor = elapsedTime / duration;
                var newColor = Color.Lerp(originalColor, finalColor, lerpFactor);
                extraFeature.GlowColor = newColor;
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
        yield break;
    }
}
