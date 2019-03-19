using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AnimatedSpotLight : AnimatedLight
{
    [SerializeField]
    public Transform lookTarget;

    [ShowOnly]
    [SerializeField]
    Vector3 lookVector = Vector3.zero;
    [ShowOnly]
    [SerializeField]
    public Transform stand;
    [ShowOnly]
    [SerializeField]
    public Transform spotlightHead;
    [ShowOnly]
    [SerializeField]
    public Transform lightElement;


    protected override void Start()
    {
        base.Start();
        lookVector = transform.position + transform.forward;
    }

    protected override void Update()
    {
        base.Update();
        if(lookTarget != null)
        {
            //framerate dependant!
            if(Application.isPlaying)
                lightElement.GetComponent<MeshRenderer>().material.color = 6f * colorGradient.Evaluate(evaluationTime / lifeTime);


            lookVector = Vector3.Lerp(lookVector, lookTarget.position, 0.08f);
            spotlightHead.LookAt(lookVector);
        }
    }

    public override void Reset()
    {
        base.Reset();
        stand = transform.FindDeepChild("Stand");
        spotlightHead = transform.FindDeepChild("SpotlightHead");
        lightElement = transform.FindDeepChild("LightElement");
        if (stand == null || spotlightHead == null || lightElement == null)
            throw new UnityException("a spotlight needs a 'Stand', a 'SpotlightHead' and a 'LightElement' gameobject");
        AnimLight.type = LightType.Spot;
    }


}
