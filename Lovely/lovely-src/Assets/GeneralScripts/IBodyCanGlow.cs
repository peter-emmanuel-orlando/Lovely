using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBodyCanGlow : IBodyFeature
{
    Color GlowColor { get; set; }
}
